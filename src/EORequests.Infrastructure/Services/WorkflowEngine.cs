using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Domain.Enums;
using EORequests.Domain.Events;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly EoDbContext _db;
        private readonly IBranchRuleEvaluator _branch;
        private readonly IDomainEventDispatcher _events;
        private readonly ILogger<WorkflowEngine> _log;

        public WorkflowEngine(EoDbContext db, IBranchRuleEvaluator branch, IDomainEventDispatcher events, ILogger<WorkflowEngine> log)
        {
            _db = db;
            _branch = branch;
            _events = events;
            _log = log;
        }

        public async Task<WorkflowInstance> StartInstanceAsync(Guid requestId, Guid startedByUserId, CancellationToken ct = default)
        {
            try
            {
                // Load request + template + first step
                var req = await _db.Requests
                    .Include(r => r.RequestType)
                    .FirstOrDefaultAsync(r => r.Id == requestId, ct)
                    ?? throw new InvalidOperationException("Request not found");

                var template = await _db.WorkflowTemplates
                    .Include(t => t.Steps)
                    .Where(t => t.RequestTypeId == req.RequestTypeId)
                    .OrderBy(t => t.CreatedOn)
                    .FirstOrDefaultAsync(ct)
                    ?? throw new InvalidOperationException("Workflow template not found for request type");

                var firstStep = template.Steps.OrderBy(s => s.StepOrder).FirstOrDefault()
                    ?? throw new InvalidOperationException("Workflow template has no steps");

                var instance = new WorkflowInstance
                {
                    RequestId = req.Id,
                    IsComplete = false
                };
                _db.WorkflowInstances.Add(instance);
                await _db.SaveChangesAsync(ct);

                var state = new WorkflowState
                {
                    WorkflowInstanceId = instance.Id,
                    StepTemplateId = firstStep.Id,
                    StateCode = WorkflowStateCode.PendingAction,
                    IsComplete = false,
                    AssigneeUserId = ComputeAssignee(firstStep, req, startedByUserId)
                };
                _db.WorkflowStates.Add(state);
                await _db.SaveChangesAsync(ct);

                instance.CurrentStepId = state.Id;
                await _db.SaveChangesAsync(ct);

                await _events.PublishAsync(new StepActivated(instance.Id, state.Id, firstStep.Id, state.AssigneeUserId), ct);
                _log.LogInformation("Workflow started for request {RequestId} with instance {InstanceId}", req.Id, instance.Id);

                return instance;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error starting workflow instance for request {RequestId}", requestId);
                throw;
            }
        }

        public async Task<(bool canAdvance, string? reason)> CanAdvanceAsync(Guid instanceId, CancellationToken ct = default)
        {
            var inst = await _db.WorkflowInstances
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.Id == instanceId, ct);

            if (inst == null) return (false, "Instance not found");
            if (inst.IsComplete) return (false, "Workflow already complete");

            var state = await _db.WorkflowStates
                .Include(s => s.WorkflowInstance)
                .Include(s => s.StepTemplate)
                .FirstOrDefaultAsync(s => s.Id == inst.CurrentStepId, ct);
            if (state == null) return (false, "Current state missing");

            if (state.IsComplete) return (false, "Current step already completed");

            // (Task gating comes in Step 10; keep placeholder reasoning here)
            var openGating = await _db.TaskItems
                .Where(t => t.WorkflowStateId == state.Id && t.IsGating && t.Status != TaskProgressStatus.Completed && t.Status != TaskProgressStatus.Cancelled)
                .AnyAsync(ct);
            if (openGating) return (false, "Gating tasks still open");

            return (true, null);
        }

        public async Task<WorkflowState> AdvanceAsync(Guid instanceId, Guid byUserId, CancellationToken ct = default)
        {
            try
            {
                var (ok, reason) = await CanAdvanceAsync(instanceId, ct);
                if (!ok) throw new InvalidOperationException(reason);

                var inst = await _db.WorkflowInstances
                    .Include(i => i.Request)
                    .FirstAsync(i => i.Id == instanceId, ct);

                var curr = await _db.WorkflowStates
                    .Include(s => s.StepTemplate)
                    .Include(s => s.WorkflowInstance)
                    .FirstAsync(s => s.Id == inst.CurrentStepId, ct);

                // complete current
                curr.IsComplete = true;
                curr.StateCode = WorkflowStateCode.Completed;
                await _db.SaveChangesAsync(ct);
                await _events.PublishAsync(new StepCompleted(inst.Id, curr.Id, curr.StepTemplateId, byUserId), ct);

                // compute next step (default = next order)
                var tmpl = await _db.WorkflowTemplates
                    .Include(t => t.Steps)
                    .FirstAsync(t => t.Id == curr.StepTemplate.WorkflowTemplateId, ct);

                var next = tmpl.Steps.OrderBy(s => s.StepOrder).FirstOrDefault(s => s.StepOrder > curr.StepTemplate.StepOrder);

                if (next == null)
                {
                    // no more steps -> complete workflow
                    inst.IsComplete = true;
                    inst.CurrentStepId = Guid.Empty;
                    await _db.SaveChangesAsync(ct);
                    return curr;
                }

                var nextState = new WorkflowState
                {
                    WorkflowInstanceId = inst.Id,
                    StepTemplateId = next.Id,
                    StateCode = WorkflowStateCode.PendingAction,
                    IsComplete = false,
                    AssigneeUserId = ComputeAssignee(next, inst.Request, byUserId)
                };
                _db.WorkflowStates.Add(nextState);
                await _db.SaveChangesAsync(ct);

                inst.CurrentStepId = nextState.Id;
                await _db.SaveChangesAsync(ct);

                await _events.PublishAsync(new StepActivated(inst.Id, nextState.Id, next.Id, nextState.AssigneeUserId), ct);

                // If assignee changed relative to previous state, emit event (informational)
                if (curr.AssigneeUserId != nextState.AssigneeUserId)
                    await _events.PublishAsync(new AssignmentChanged(inst.Id, nextState.Id, curr.AssigneeUserId, nextState.AssigneeUserId), ct);

                return nextState;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Advance failed for instance {InstanceId}", instanceId);
                throw;
            }
        }

        public async Task<WorkflowState> SkipOrBranchAsync(Guid instanceId, string ruleKey, Guid byUserId, CancellationToken ct = default)
        {
            try
            {
                var (ok, reason) = await CanAdvanceAsync(instanceId, ct);
                if (!ok) throw new InvalidOperationException(reason);

                var inst = await _db.WorkflowInstances
                    .Include(i => i.Request)
                    .FirstAsync(i => i.Id == instanceId, ct);

                var curr = await _db.WorkflowStates
                    .Include(s => s.StepTemplate)
                    .Include(s => s.WorkflowInstance)
                    .FirstAsync(s => s.Id == inst.CurrentStepId, ct);

                // complete current step (considered "skipped/branched")
                curr.IsComplete = true;
                curr.StateCode = WorkflowStateCode.Skipped;
                await _db.SaveChangesAsync(ct);
                await _events.PublishAsync(new StepCompleted(inst.Id, curr.Id, curr.StepTemplateId, byUserId), ct);

                var tmpl = await _db.WorkflowTemplates
                    .Include(t => t.Steps)
                    .FirstAsync(t => t.Id == curr.StepTemplate.WorkflowTemplateId, ct);

                var desiredOrder = await _branch.EvaluateNextStepOrderAsync(curr, ruleKey, ct);
                WorkflowStepTemplate? nextTmpl = null;

                if (desiredOrder == -1)
                {
                    inst.IsComplete = true;
                    inst.CurrentStepId = Guid.Empty;
                    await _db.SaveChangesAsync(ct);
                    return curr;
                }
                else if (desiredOrder.HasValue)
                {
                    nextTmpl = tmpl.Steps.FirstOrDefault(s => s.StepOrder == desiredOrder.Value);
                }
                else
                {
                    nextTmpl = tmpl.Steps.OrderBy(s => s.StepOrder).FirstOrDefault(s => s.StepOrder > curr.StepTemplate.StepOrder);
                }

                if (nextTmpl == null)
                {
                    inst.IsComplete = true;
                    inst.CurrentStepId = Guid.Empty;
                    await _db.SaveChangesAsync(ct);
                    return curr;
                }

                var nextState = new WorkflowState
                {
                    WorkflowInstanceId = inst.Id,
                    StepTemplateId = nextTmpl.Id,
                    StateCode = WorkflowStateCode.PendingAction,
                    IsComplete = false,
                    AssigneeUserId = ComputeAssignee(nextTmpl, inst.Request, byUserId)
                };
                _db.WorkflowStates.Add(nextState);
                await _db.SaveChangesAsync(ct);

                inst.CurrentStepId = nextState.Id;
                await _db.SaveChangesAsync(ct);

                await _events.PublishAsync(new StepActivated(inst.Id, nextState.Id, nextTmpl.Id, nextState.AssigneeUserId), ct);
                if (curr.AssigneeUserId != nextState.AssigneeUserId)
                    await _events.PublishAsync(new AssignmentChanged(inst.Id, nextState.Id, curr.AssigneeUserId, nextState.AssigneeUserId), ct);

                return nextState;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Skip/Branch failed for instance {InstanceId} with rule {Rule}", instanceId, ruleKey);
                throw;
            }
        }

        private static Guid? ComputeAssignee(WorkflowStepTemplate step, Request req, Guid actorUserId)
        {
            return step.AssignmentMode switch
            {
                AssignmentMode.AutoAssign => req.CreatedByUserId, // v1: assign to request creator
                AssignmentMode.SelectedByPreviousStep => actorUserId,
                AssignmentMode.RoleBased => null, // assignee determined by role at runtime
                _ => null
            };
        }
    }
}

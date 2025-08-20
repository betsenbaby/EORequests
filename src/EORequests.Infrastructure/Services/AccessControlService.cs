using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Domain.Enums;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


//The rules enforce:
//Admin can do everything.
//Role‑based via WorkflowStepTemplate.AllowedRolesCsv.
//Specific assignee via WorkflowState.AssigneeUserId.
//Creator/Preparer via WorkflowStepTemplate.AllowCreatorOrPreparer.
//Act blocked when IsComplete = true.

namespace EORequests.Infrastructure.Services
{
    public class AccessControlService : IAccessControlService
    {
        private static readonly string AdminRole = "Admin";
        private readonly EoDbContext _db;
        private readonly ILogger<AccessControlService> _log;

        public AccessControlService(EoDbContext db, ILogger<AccessControlService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task<bool> CanPerformAsync(Guid workflowStateId, Guid userId, IEnumerable<string> roles, StepAction action, CancellationToken ct = default)
        {
            var roleSet = roles?.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim())
                                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Admin override
            if (roleSet.Contains(AdminRole))
                return true;

            // Load minimal graph needed
            var ws = await _db.WorkflowStates
                .AsNoTracking()
                .Include(s => s.WorkflowInstance)
                    .ThenInclude(wi => wi.Request)
                .Include(s => s.StepTemplate)
                .FirstOrDefaultAsync(s => s.Id == workflowStateId, ct);

            if (ws == null)
                return false;

            var step = ws.StepTemplate;
            var request = ws.WorkflowInstance.Request;

            // Finished steps: allow read‑only things, block "Act"
            if (ws.IsComplete && action == StepAction.Act)
                return false;

            // Everyone who can see the request can View/Comment/UploadAttachment (tweak as needed)
            // Start with View = true if user is directly involved
            var canView = false;

            // 1) Creator/Preparer visibility
            if (request.CreatedByUserId == userId || request.PreparedByUserId == userId)
                canView = true;

            // 2) Specific assignee visibility
            if (ws.AssigneeUserId.HasValue && ws.AssigneeUserId.Value == userId)
                canView = true;

            // 3) Role‑based visibility
            if (!string.IsNullOrWhiteSpace(step.AllowedRolesCsv) && roleSet.Count > 0)
            {
                var allowedRoles = step.AllowedRolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (allowedRoles.Any(ar => roleSet.Contains(ar)))
                    canView = true;
            }

            // "View" permission short‑circuit
            if (action == StepAction.View)
                return canView;

            // ---- ACT permission rules ----
            if (action == StepAction.Act)
            {
                // If step allows creator/preparer, that’s enough
                if (step.AllowCreatorOrPreparer && (request.CreatedByUserId == userId || request.PreparedByUserId == userId))
                    return true;

                // If this state is assigned to a user (SelectedByPreviousStep/AutoAssign flow)
                if (ws.AssigneeUserId.HasValue && ws.AssigneeUserId.Value == userId)
                    return true;

                // Role‑based steps
                if (!string.IsNullOrWhiteSpace(step.AllowedRolesCsv) && roleSet.Count > 0)
                {
                    var allowedRoles = step.AllowedRolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (allowedRoles.Any(ar => roleSet.Contains(ar)))
                        return true;
                }

                return false;
            }

            // ---- UploadAttachment / Comment / CreateTask ----
            // Tie these to canView by default; tighten if needed
            if (action is StepAction.UploadAttachment or StepAction.Comment or StepAction.CreateTask)
                return canView;

            // ---- AssignStep (only when SelectedByPreviousStep) ----
            if (action == StepAction.AssignStep)
            {
                // Only the actor who just completed the previous step OR a ProcessOwner/Admin role typically
                // Here: allow if user can Act on this step OR has "ProcessOwner"
                if (roleSet.Contains("ProcessOwner"))
                    return true;

                // If the state is pending and user is the assignee, let them assign onward
                if (!ws.IsComplete && ws.AssigneeUserId == userId)
                    return true;

                return false;
            }

            return false;
        }

        public async Task<IReadOnlyCollection<StepAction>> GetAllowedActionsAsync(Guid workflowStateId, Guid userId, IEnumerable<string> roles, CancellationToken ct = default)
        {
            var rights = new List<StepAction>
            {
                StepAction.View,
                StepAction.Comment,
                StepAction.UploadAttachment,
                StepAction.CreateTask,
                StepAction.Act,
                StepAction.AssignStep
            };

            var result = new List<StepAction>();
            foreach (var action in rights)
            {
                if (await CanPerformAsync(workflowStateId, userId, roles, action, ct))
                    result.Add(action);
            }
            return result;
        }
    }
}


using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Domain.Enums;
using EORequests.Infrastructure.Data;
using EORequests.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EORequests.Tests
{
    public class SlaIntegrationTests
    {
        private static EoDbContext InMemoryDb()
        {
            var options = new DbContextOptionsBuilder<EoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new EoDbContext(options, new HttpContextAccessorStub());
        }

        private static async Task SeedConsultantAsync(EoDbContext db)
        {
            var rt = new RequestType { Code = "CONSULT", Name = "Consultant" };
            db.RequestTypes.Add(rt);
            await db.SaveChangesAsync();

            var tmpl = new WorkflowTemplate { RequestTypeId = rt.Id, Code = "CONSULT_V1", Name = "Consult Workflow V1" };
            db.WorkflowTemplates.Add(tmpl);
            await db.SaveChangesAsync();

            db.WorkflowStepTemplates.AddRange(
                new WorkflowStepTemplate { WorkflowTemplateId = tmpl.Id, StepOrder = 1, Code = "SUBMIT", Name = "Submit", AssignmentMode = AssignmentMode.AutoAssign, AllowCreatorOrPreparer = true },
                new WorkflowStepTemplate { WorkflowTemplateId = tmpl.Id, StepOrder = 2, Code = "REVIEW", Name = "Review", AssignmentMode = AssignmentMode.RoleBased, AllowedRolesCsv = "Reviewer" },
                new WorkflowStepTemplate { WorkflowTemplateId = tmpl.Id, StepOrder = 3, Code = "APPROVE", Name = "Approve", AssignmentMode = AssignmentMode.SelectedByPreviousStep }
            );
            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task StartInstance_Calls_SLA_Compute_And_Schedule()
        {
            using var db = InMemoryDb();
            await SeedConsultantAsync(db);

            var request = new Request
            {
                RequestTypeId = db.RequestTypes.Single().Id,
                CreatedByUserId = Guid.NewGuid(),
                PreparedByUserId = Guid.NewGuid(),
                Title = "Test"
            };
            db.Requests.Add(request);
            await db.SaveChangesAsync();

            // Mocks
            var sla = new Mock<ISlaService>(MockBehavior.Strict);
            WorkflowState? capturedState = null;

            sla.Setup(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .Callback<WorkflowState, CancellationToken>((st, _) => capturedState = st)
               .ReturnsAsync((DateTime?)null)
               .Verifiable();

            sla.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Verifiable();

            var engine = new WorkflowEngine(
                db,
                new BranchRuleEvaluator(),
                new LoggingEventDispatcher(new NullLogger<LoggingEventDispatcher>()),
                sla.Object,
                new NullLogger<WorkflowEngine>());

            var instance = await engine.StartInstanceAsync(request.Id, request.CreatedByUserId);

            Assert.NotNull(capturedState);
            Assert.Equal(instance.Id, capturedState!.WorkflowInstanceId);

            sla.Verify(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.Verify(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Advance_Cancels_Current_SLA_And_Schedules_Next()
        {
            using var db = InMemoryDb();
            await SeedConsultantAsync(db);

            var requester = Guid.NewGuid();
            var request = new Request
            {
                RequestTypeId = db.RequestTypes.Single().Id,
                CreatedByUserId = requester,
                PreparedByUserId = requester,
                Title = "Advance Test"
            };
            db.Requests.Add(request);
            await db.SaveChangesAsync();

            // Strict mock to ensure exact calls
            var sla = new Mock<ISlaService>(MockBehavior.Strict);

            // When starting, engine will compute/schedule for step1
            sla.Setup(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((DateTime?)null);
            sla.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var engine = new WorkflowEngine(
                db,
                new BranchRuleEvaluator(),
                new LoggingEventDispatcher(new NullLogger<LoggingEventDispatcher>()),
                sla.Object,
                new NullLogger<WorkflowEngine>());

            var instance = await engine.StartInstanceAsync(request.Id, requester);

            // Capture current state before advance
            var curr = await db.WorkflowStates.FirstAsync(s => s.Id == instance.CurrentStepId);

            // Expect: cancel for current, then compute/schedule for next
            sla.Invocations.Clear();
            sla.Setup(s => s.CancelJobsForStateAsync(curr.Id)).Returns(Task.CompletedTask).Verifiable();
            sla.Setup(s => s.ComputeAndSetDueDateAsync(It.Is<WorkflowState>(st => st.Id != curr.Id && st.WorkflowInstanceId == instance.Id), It.IsAny<CancellationToken>()))
               .ReturnsAsync((DateTime?)null)
               .Verifiable();
            sla.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.Is<WorkflowState>(st => st.Id != curr.Id && st.WorkflowInstanceId == instance.Id), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Verifiable();

            var next = await engine.AdvanceAsync(instance.Id, byUserId: Guid.NewGuid());

            Assert.NotEqual(curr.Id, next.Id);

            sla.Verify(s => s.CancelJobsForStateAsync(curr.Id), Times.Once);
            sla.Verify(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.Verify(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Branch_Cancels_Current_SLA_And_Schedules_Branched_Next()
        {
            using var db = InMemoryDb();
            await SeedConsultantAsync(db);

            var requester = Guid.NewGuid();
            var request = new Request
            {
                RequestTypeId = db.RequestTypes.Single().Id,
                CreatedByUserId = requester,
                PreparedByUserId = requester,
                Title = "Branch Test"
            };
            db.Requests.Add(request);
            await db.SaveChangesAsync();

            // initial SLA at start
            var sla = new Mock<ISlaService>(MockBehavior.Strict);
            sla.Setup(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((DateTime?)null);
            sla.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var engine = new WorkflowEngine(
                db,
                new BranchRuleEvaluator(),
                new LoggingEventDispatcher(new NullLogger<LoggingEventDispatcher>()),
                sla.Object,
                new NullLogger<WorkflowEngine>());

            var instance = await engine.StartInstanceAsync(request.Id, requester);

            var curr = await db.WorkflowStates.FirstAsync(s => s.Id == instance.CurrentStepId);

            // Reset and set expectations for branching to order 3
            sla.Invocations.Clear();
            sla.Setup(s => s.CancelJobsForStateAsync(curr.Id)).Returns(Task.CompletedTask).Verifiable();
            sla.Setup(s => s.ComputeAndSetDueDateAsync(It.Is<WorkflowState>(st => st.Id != curr.Id && st.WorkflowInstanceId == instance.Id), It.IsAny<CancellationToken>()))
               .ReturnsAsync((DateTime?)null)
               .Verifiable();
            sla.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.Is<WorkflowState>(st => st.Id != curr.Id && st.WorkflowInstanceId == instance.Id), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask)
               .Verifiable();

            var branched = await engine.SkipOrBranchAsync(instance.Id, "goto:3", byUserId: requester);

            Assert.NotEqual(curr.Id, branched.Id);

            sla.Verify(s => s.CancelJobsForStateAsync(curr.Id), Times.Once);
            sla.Verify(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.Verify(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()), Times.Once);
            sla.VerifyNoOtherCalls();
        }

        private sealed class HttpContextAccessorStub : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
        }
    }
}

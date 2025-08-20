using EORequests.Domain.Entities;
using EORequests.Domain.Enums;
using EORequests.Infrastructure.Data;
using EORequests.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Moq; // if you want to use Moq; otherwise see No‑Moq stub below
using EORequests.Application.Interfaces;

namespace EORequests.Tests
{
    public class WorkflowEngineTests
    {
        private static EoDbContext InMemoryDb()
        {
            var options = new DbContextOptionsBuilder<EoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // EoDbContext requires IHttpContextAccessor; provide a tiny stub
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
        public async Task Start_Then_Advance_Through_All_Steps()
        {
            using var db = InMemoryDb();
            await SeedConsultantAsync(db);

            var requesterId = Guid.NewGuid();
            var req = new Request
            {
                RequestTypeId = db.RequestTypes.Single().Id,
                CreatedByUserId = requesterId,
                PreparedByUserId = requesterId,
                Title = "Test Request"
            };
            db.Requests.Add(req);
            await db.SaveChangesAsync();

            var slaMock = new Moq.Mock<ISlaService>(Moq.MockBehavior.Loose);
            slaMock.Setup(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((DateTime?)null);
            slaMock.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
            slaMock.Setup(s => s.CancelJobsForStateAsync(It.IsAny<Guid>()))
                   .Returns(Task.CompletedTask);

            var engine = new WorkflowEngine(
                db,
                new BranchRuleEvaluator(),
                new LoggingEventDispatcher(new NullLogger<LoggingEventDispatcher>()),
                slaMock.Object,
                new NullLogger<WorkflowEngine>());


            var instance = await engine.StartInstanceAsync(req.Id, requesterId);
            Assert.False(instance.IsComplete);
            Assert.NotEqual(Guid.Empty, instance.CurrentStepId);

            var (canAdvance, reason) = await engine.CanAdvanceAsync(instance.Id);
            Assert.True(canAdvance, reason);

            var state2 = await engine.AdvanceAsync(instance.Id, byUserId: Guid.NewGuid());
            Assert.Equal("REVIEW", db.WorkflowStepTemplates.Find(state2.StepTemplateId)!.Code);

            var state3 = await engine.AdvanceAsync(instance.Id, byUserId: Guid.NewGuid());
            Assert.Equal("APPROVE", db.WorkflowStepTemplates.Find(state3.StepTemplateId)!.Code);

            // Final advance completes workflow
            await engine.AdvanceAsync(instance.Id, byUserId: Guid.NewGuid());

            var reloaded = db.WorkflowInstances.Find(instance.Id)!;
            Assert.True(reloaded.IsComplete);
            Assert.Equal(Guid.Empty, reloaded.CurrentStepId);
        }

        [Fact]
        public async Task Branch_To_Specific_Order()
        {
            using var db = InMemoryDb();
            await SeedConsultantAsync(db);

            var req = new Request
            {
                RequestTypeId = db.RequestTypes.Single().Id,
                CreatedByUserId = Guid.NewGuid(),
                Title = "Branch Test"
            };
            db.Requests.Add(req);
            await db.SaveChangesAsync();

            var slaMock = new Moq.Mock<ISlaService>(Moq.MockBehavior.Loose);
            slaMock.Setup(s => s.ComputeAndSetDueDateAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((DateTime?)null);
            slaMock.Setup(s => s.ScheduleReminderAndEscalationJobsAsync(It.IsAny<WorkflowState>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);
            slaMock.Setup(s => s.CancelJobsForStateAsync(It.IsAny<Guid>()))
                   .Returns(Task.CompletedTask);

            var engine = new WorkflowEngine(
                db,
                new BranchRuleEvaluator(),
                new LoggingEventDispatcher(new NullLogger<LoggingEventDispatcher>()),
                slaMock.Object,
                new NullLogger<WorkflowEngine>());


            var inst = await engine.StartInstanceAsync(req.Id, req.CreatedByUserId);

            // Jump from step1 to step3
            var s3 = await engine.SkipOrBranchAsync(inst.Id, "goto:3", byUserId: Guid.NewGuid());
            Assert.Equal("APPROVE", db.WorkflowStepTemplates.Find(s3.StepTemplateId)!.Code);
        }

        // Minimal IHttpContextAccessor stub for EoDbContext ctor
        private sealed class HttpContextAccessorStub : IHttpContextAccessor
        {
            public HttpContext? HttpContext { get; set; }
        }
    }
}

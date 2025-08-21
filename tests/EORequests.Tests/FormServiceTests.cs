using EORequests.Infrastructure.Data;
using EORequests.Infrastructure.Services;
using EORequests.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class FormServiceTests
{
    private static EoDbContext Db()
    {
        var opts = new DbContextOptionsBuilder<EoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EoDbContext(opts, new HttpContextAccessor());
    }

    [Fact]
    public async Task Upsert_Saves_Data_And_Captures_Version()
    {
        using var db = Db();

        var rt = new RequestType { Code = "CONSULT", Name = "Consultant" };
        db.RequestTypes.Add(rt); await db.SaveChangesAsync();

        var tpl = new WorkflowTemplate { RequestTypeId = rt.Id, Code = "CONSULT_V1", Name = "Consult" };
        db.WorkflowTemplates.Add(tpl); await db.SaveChangesAsync();

        var step = new WorkflowStepTemplate { WorkflowTemplateId = tpl.Id, StepOrder = 1, Code = "CR_SUBMIT", Name = "Submit", JsonSchemaVersion = "v1" };
        db.WorkflowStepTemplates.Add(step); await db.SaveChangesAsync();

        var req = new Request { RequestTypeId = rt.Id, Title = "T", CreatedByUserId = Guid.NewGuid() };
        db.Requests.Add(req); await db.SaveChangesAsync();

        var inst = new WorkflowInstance { RequestId = req.Id };
        db.WorkflowInstances.Add(inst); await db.SaveChangesAsync();

        var state = new WorkflowState { WorkflowInstanceId = inst.Id, StepTemplateId = step.Id };
        db.WorkflowStates.Add(state); await db.SaveChangesAsync();

        var svc = new FormService(db, new NullLogger<FormService>());

        // First save
        await svc.UpsertResponseAsync(state.Id, """{"title":"Hello"}""", "sum");
        var r1 = await db.FormResponses.SingleAsync();
        Assert.Equal("v1", r1.SchemaVersionCaptured);
        Assert.Contains("Hello", r1.JsonData);

        // Change version at step and save again
        step.JsonSchemaVersion = "v2";
        await db.SaveChangesAsync();

        await svc.UpsertResponseAsync(state.Id, """{"title":"World"}""", "sum2");
        var r2 = await db.FormResponses.SingleAsync();
        Assert.Equal("v2", r2.SchemaVersionCaptured); // if you prefer freezing, change the assertion accordingly
        Assert.Contains("World", r2.JsonData);
    }
}

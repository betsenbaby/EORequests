using EORequests.Application.Interfaces;
using EORequests.Infrastructure.Data;
using EORequests.Web.Contracts;
using EORequests.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static EORequests.Web.Components.Admin.StepSchemaEditor;

namespace EORequests.Web.Controllers.Admin
{
    [Route("api/admin/steps/{stepId:guid}/schema")]
    [Authorize(Policy = "AdminOnly")]
    public sealed class StepSchemaController : BaseApiController<StepSchemaController>
    {
        private readonly IWorkflowStepTemplateService _svc;

        public StepSchemaController(IWorkflowStepTemplateService svc, ILogger<StepSchemaController> log) : base(log)
        {
            _svc = svc;
        }

        [HttpGet]
        public Task<IActionResult> Get(Guid stepId) => RunAsync(async () =>
        {
            var schema = await _svc.GetSchemaAsync(stepId, HttpContext.RequestAborted);
            return Ok(new StepSchemaDto { StepTemplateId = stepId, JsonSchema = schema });
        }, activity: $"GET /api/admin/steps/{stepId}/schema");

        [HttpPut]
        public Task<IActionResult> Put(Guid stepId, [FromBody] StepSchemaDto dto) => RunAsync(async () =>
        {
            if (dto.StepTemplateId != stepId)
                return BadRequest(new { error = "Mismatched ids" });

            // minimal validation: if non-empty, ensure it's valid JSON
            if (!string.IsNullOrWhiteSpace(dto.JsonSchema))
                System.Text.Json.JsonDocument.Parse(dto.JsonSchema);

            await _svc.UpdateSchemaAsync(stepId, dto.JsonSchema, HttpContext.RequestAborted);
            return Ok(new { ok = true });
        }, activity: $"PUT /api/admin/steps/{stepId}/schema");
    }
}

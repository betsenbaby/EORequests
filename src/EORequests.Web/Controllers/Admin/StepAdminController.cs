using EORequests.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EORequests.Web.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/steps")]
    public class StepAdminController : ControllerBase
    {
        private readonly EoDbContext _db;
        public StepAdminController(EoDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var steps = await _db.WorkflowStepTemplates
                .Include(s => s.WorkflowTemplate)
                .OrderBy(s => s.WorkflowTemplate.Name)
                .ThenBy(s => s.StepOrder)
                .ToListAsync();

            return Ok(steps);
        }
    }
}

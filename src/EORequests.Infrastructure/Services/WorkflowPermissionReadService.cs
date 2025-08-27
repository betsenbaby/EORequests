using EORequests.Application.Interfaces;
using EORequests.Domain.Enums;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public sealed class WorkflowPermissionReadService : IWorkflowPermissionReadService
    {
        private readonly EoDbContext _db;
        public WorkflowPermissionReadService(EoDbContext db) => _db = db;

        public async Task<IReadOnlyList<PermissionMatrixRow>> GetMatrixAsync(Guid workflowTemplateId, CancellationToken ct = default)
        {
            // Pull only what we need from EF
            var steps = await _db.WorkflowStepTemplates
                .Where(s => s.WorkflowTemplateId == workflowTemplateId)
                .OrderBy(s => s.StepOrder)
                .Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.Name,
                    s.AllowedRolesCsv,
                    s.AllowCreatorOrPreparer,
                    s.AssignmentMode // enum EORequests.Domain.Enums.AssignmentMode
                })
                .AsNoTracking()
                .ToListAsync(ct);

            // Collect all roles mentioned across steps
            var allRoles = steps
                .SelectMany(s => (s.AllowedRolesCsv ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var rows = new List<PermissionMatrixRow>();

            foreach (var s in steps)
            {
                var isSelectedByPrev = s.AssignmentMode == AssignmentMode.SelectedByPreviousStep;

                var roleMap = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (var role in allRoles)
                {
                    var allowed = (s.AllowedRolesCsv ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));

                    // You can later surface AllowCreatorOrPreparer / isSelectedByPrev in UI if desired
                    roleMap[role] = allowed;
                }

                rows.Add(new PermissionMatrixRow(
                    StepId: s.Id,
                    StepCode: s.Code ?? "",
                    StepName: s.Name ?? "",
                    RoleCanAct: roleMap
                ));
            }

            return rows;
        }
    }
}

using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IWorkflowStepTemplateService
    {
        Task<IReadOnlyList<(WorkflowStepTemplate Step, string TemplateName)>> GetAllAsync(CancellationToken ct = default);
        Task<string?> GetSchemaAsync(Guid stepTemplateId, CancellationToken ct = default);
        Task UpdateSchemaAsync(Guid stepTemplateId, string? jsonSchema, CancellationToken ct = default);
    }
}

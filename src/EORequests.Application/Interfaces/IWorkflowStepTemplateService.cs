using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public sealed record StepListItem(
    Guid Id, int StepOrder, string Code, string Name, string AssignmentMode, bool AllowCreatorOrPreparer, bool HasSchema);

    public sealed record StepEditDto(
        Guid Id, Guid WorkflowTemplateId, int StepOrder, string Code, string Name,
        string AssignmentMode, string? AllowedRolesCsv, bool AllowCreatorOrPreparer);

    public interface IWorkflowStepTemplateService
    {
        Task<IReadOnlyList<StepListItem>> ListForTemplateAsync(Guid workflowTemplateId, CancellationToken ct = default);
        Task<StepEditDto?> GetAsync(Guid stepId, CancellationToken ct = default);
        Task<Guid> CreateAsync(StepEditDto dto, CancellationToken ct = default);
        Task UpdateAsync(StepEditDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid stepId, CancellationToken ct = default);
        Task RenumberAsync(Guid workflowTemplateId, CancellationToken ct = default);

        Task<IReadOnlyList<(WorkflowStepTemplate Step, string TemplateName)>> GetAllAsync(CancellationToken ct = default);
        Task<string?> GetSchemaAsync(Guid stepTemplateId, CancellationToken ct = default);
        Task UpdateSchemaAsync(Guid stepTemplateId, string? jsonSchema, CancellationToken ct = default);
    }
}

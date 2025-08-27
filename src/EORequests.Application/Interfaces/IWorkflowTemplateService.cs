using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{

    public sealed record WorkflowTemplateListItem(
    Guid Id, string Code, string Name, string RequestTypeName, bool IsActive, DateTime CreatedOn);

    public sealed record WorkflowTemplateDto(
        Guid Id, Guid RequestTypeId, string Code, string Name, bool IsActive);

    public interface IWorkflowTemplateService
    {
        Task<IReadOnlyList<WorkflowTemplateListItem>> ListAsync(string? search = null, CancellationToken ct = default);
        Task<WorkflowTemplateDto?> GetAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateAsync(WorkflowTemplateDto dto, CancellationToken ct = default);
        Task UpdateAsync(WorkflowTemplateDto dto, CancellationToken ct = default);
        Task ArchiveAsync(Guid id, CancellationToken ct = default);

        Task<IReadOnlyList<(Guid Id, string Name)>> ListRequestTypesAsync(CancellationToken ct = default);
    }
}

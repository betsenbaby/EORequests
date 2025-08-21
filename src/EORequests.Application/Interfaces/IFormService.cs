using EORequests.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IFormService
    {
        Task<string?> GetSchemaAsync(Guid workflowStateId, CancellationToken ct = default);
        Task<FormResponse?> GetResponseAsync(Guid workflowStateId, CancellationToken ct = default);
        Task<FormResponse> UpsertResponseAsync(Guid workflowStateId, string jsonData, string? summary = null, CancellationToken ct = default);
    }
}

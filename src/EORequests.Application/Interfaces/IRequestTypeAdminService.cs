using EORequests.Application.DTOs.RequestType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IRequestTypeAdminService
    {
        Task<IReadOnlyList<RequestTypeDto>> GetAllAsync(CancellationToken ct = default);
        Task<RequestTypeDto?> GetAsync(Guid id, CancellationToken ct = default);
        Task<Guid> UpsertAsync(RequestTypeUpsertDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}

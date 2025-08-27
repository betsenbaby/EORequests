using EORequests.Application.DTOs.RequestType;
using EORequests.Application.Interfaces;
using EORequests.Domain.Entities;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public sealed class RequestTypeAdminService : IRequestTypeAdminService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<RequestTypeAdminService> _log;

        public RequestTypeAdminService(EoDbContext db, ILogger<RequestTypeAdminService> log)
        {
            _db = db; _log = log;
        }

        public async Task<IReadOnlyList<RequestTypeDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.RequestTypes
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new RequestTypeDto { Id = x.Id, Code = x.Code!, Name = x.Name! })
                .ToListAsync(ct);
        }

        public async Task<RequestTypeDto?> GetAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.RequestTypes
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new RequestTypeDto { Id = x.Id, Code = x.Code!, Name = x.Name! })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<Guid> UpsertAsync(RequestTypeUpsertDto dto, CancellationToken ct = default)
        {
            // Normalize
            dto.Code = dto.Code?.Trim() ?? "";
            dto.Name = dto.Name?.Trim() ?? "";

            var isCreate = !dto.Id.HasValue || dto.Id.Value == Guid.Empty;

            if (isCreate)
            {
                var e = new RequestType
                {
                    Code = dto.Code,
                    Name = dto.Name
                };
                _db.RequestTypes.Add(e);
                await _db.SaveChangesAsync(ct);
                return e.Id;
            }

            var id = dto.Id!.Value;
            var entity = await _db.RequestTypes.FindAsync([id], ct); // if this syntax complains, use: new object[] { id }
            if (entity is null) throw new InvalidOperationException("Request Type not found.");

            entity.Code = dto.Code;
            entity.Name = dto.Name;

            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }


        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _db.RequestTypes.FindAsync([id], ct);
            if (entity is null) return;
            _db.RequestTypes.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}

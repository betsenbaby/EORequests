using Microsoft.Extensions.Logging;
using EORequests.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EORequests.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EORequests.Infrastructure.Interfaces
{
    public class WorkflowTemplateService : IWorkflowTemplateService
    {
        private readonly EoDbContext _db;
        private readonly ILogger<WorkflowTemplateService> _log;
        public WorkflowTemplateService(EoDbContext db, ILogger<WorkflowTemplateService> log)
        { _db = db; _log = log; }

        public async Task<IReadOnlyList<WorkflowTemplateListItem>> ListAsync(
            string? search, CancellationToken ct = default)
        {
            var q = _db.WorkflowTemplates
                .AsNoTracking()
                .Include(t => t.RequestType)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(t =>
                    t.Code!.Contains(s) ||
                    t.Name!.Contains(s) ||
                    t.RequestType.Name.Contains(s));
            }

            return await q
                .OrderByDescending(t => t.CreatedOn)
                .Select(t => new WorkflowTemplateListItem(
                    t.Id, t.Code!, t.Name!, t.RequestType.Name, t.IsActive, t.CreatedOn))
                .ToListAsync(ct);
        }


        public async Task<WorkflowTemplateDto?> GetAsync(Guid id, CancellationToken ct = default)
            => await _db.WorkflowTemplates
                .AsNoTracking()
                .Where(t => t.Id == id)                          // filter FIRST
                .Select(t => new WorkflowTemplateDto(            // then project
                    t.Id, t.RequestTypeId, t.Code!, t.Name!, t.IsActive))
                .FirstOrDefaultAsync(ct);


        public async Task<Guid> CreateAsync(WorkflowTemplateDto dto, CancellationToken ct = default)
        {
            var entity = new EORequests.Domain.Entities.WorkflowTemplate
            {
                RequestTypeId = dto.RequestTypeId,
                Code = dto.Code,
                Name = dto.Name,
                IsActive = dto.IsActive
            };
            _db.WorkflowTemplates.Add(entity);
            await _db.SaveChangesAsync(ct);
            return entity.Id;
        }

        public async Task UpdateAsync(WorkflowTemplateDto dto, CancellationToken ct = default)
        {
            var e = await _db.WorkflowTemplates.FirstAsync(x => x.Id == dto.Id, ct);
            e.RequestTypeId = dto.RequestTypeId;
            e.Code = dto.Code;
            e.Name = dto.Name;
            e.IsActive = dto.IsActive;
            await _db.SaveChangesAsync(ct);
        }

        public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
        {
            var e = await _db.WorkflowTemplates.FirstAsync(x => x.Id == id, ct);
            e.IsActive = false;
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<(Guid Id, string Name)>> ListRequestTypesAsync(CancellationToken ct = default)
            => await _db.RequestTypes
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new ValueTuple<Guid, string>(r.Id, r.Name))
                .ToListAsync(ct);
    }
}

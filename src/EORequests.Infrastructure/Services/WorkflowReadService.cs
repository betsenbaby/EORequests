using EORequests.Application.Interfaces;
using EORequests.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Services
{
    public class WorkflowReadService : IWorkflowReadService
    {
        private readonly EoDbContext _db;
        public WorkflowReadService(EoDbContext db) => _db = db;

        public async Task<Guid> GetInstanceIdByStateIdAsync(Guid stateId, CancellationToken ct = default)
            => await _db.WorkflowStates
                        .Where(s => s.Id == stateId)
                        .Select(s => s.WorkflowInstanceId)
                        .FirstAsync(ct);

        // NEW
        public async Task<Guid> GetRequestIdByStateIdAsync(Guid stateId, CancellationToken ct = default)
            => await _db.WorkflowStates
                        .Where(s => s.Id == stateId)
                        .Select(s => s.WorkflowInstance.RequestId)
                        .FirstAsync(ct);
    }
}

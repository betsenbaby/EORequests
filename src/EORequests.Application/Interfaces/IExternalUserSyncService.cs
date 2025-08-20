using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IExternalUserSyncService
    {
        Task<int> SyncUsersAsync(CancellationToken ct = default);
    }
}

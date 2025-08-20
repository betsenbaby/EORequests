using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IDomainEventDispatcher
    {
        Task PublishAsync<T>(T evt, CancellationToken ct = default);
    }
}

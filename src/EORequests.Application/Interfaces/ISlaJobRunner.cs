using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface ISlaJobRunner
    {
        Task SendReminder(Guid workflowStateId, int daysBeforeDue);
        Task Escalate(Guid workflowStateId);
    }
}

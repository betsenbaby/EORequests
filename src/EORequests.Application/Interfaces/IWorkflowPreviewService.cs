using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Application.Interfaces
{
    public interface IWorkflowPreviewService
    {
        /// <summary>
        /// Creates a throwaway Request for the step's template RequestType, starts a WorkflowInstance,
        /// advances up to the specified step, and returns the active WorkflowStateId ready for filling.
        /// </summary>
        Task<Guid?> CreatePreviewAtStepAsync(Guid stepTemplateId, Guid byUserId, CancellationToken ct = default);
    }
}

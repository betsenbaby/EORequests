using EORequests.Domain.Enums;

namespace EORequests.Application.Interfaces
{
    public interface IAccessControlService
    {
        Task<bool> CanPerformAsync(Guid workflowStateId, Guid userId, IEnumerable<string> roles, StepAction action, CancellationToken ct = default);

        /// <summary>Convenience to fetch once and compute all actions the caller can do on a state.</summary>
        Task<IReadOnlyCollection<StepAction>> GetAllowedActionsAsync(Guid workflowStateId, Guid userId, IEnumerable<string> roles, CancellationToken ct = default);
    }
}

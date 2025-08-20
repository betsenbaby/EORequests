using EORequests.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace EORequests.Web.Security
{
    public sealed class StepActionRequirement : IAuthorizationRequirement
    {
        public StepActionRequirement(StepAction action) => Action = action;
        public StepAction Action { get; }
    }

    public sealed class WorkflowStateResource
    {
        public Guid WorkflowStateId { get; init; }
        public Guid UserId { get; init; }
        public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
    }
}

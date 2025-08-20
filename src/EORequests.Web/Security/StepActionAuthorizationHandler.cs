using EORequests.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EORequests.Web.Security
{
    public sealed class StepActionAuthorizationHandler : AuthorizationHandler<StepActionRequirement, WorkflowStateResource>
    {
        private readonly IAccessControlService _acs;
        private readonly ILogger<StepActionAuthorizationHandler> _log;
        public StepActionAuthorizationHandler(IAccessControlService acs, ILogger<StepActionAuthorizationHandler> log)
        {
            _acs = acs;
            _log = log;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, StepActionRequirement requirement, WorkflowStateResource resource)
        {
            var ok = await _acs.CanPerformAsync(resource.WorkflowStateId, resource.UserId, resource.Roles, requirement.Action);
            if (ok) context.Succeed(requirement);
        }
    }
}

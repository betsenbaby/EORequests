using EORequests.Web.Infrastructure;
using EORequests.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EORequests.Web.Controllers
{
    [Route("api/workflow")]
    public class WorkflowActionsController : BaseApiController<WorkflowActionsController>
    {
        private readonly IAuthorizationService _auth;
        private readonly ICurrentUser _me;

        public WorkflowActionsController(
            IAuthorizationService auth,
            ICurrentUser me,
            ILogger<WorkflowActionsController> log) : base(log)
        {
            _auth = auth ?? throw new ArgumentNullException(nameof(auth));
            _me = me ?? throw new ArgumentNullException(nameof(me));
        }

        [HttpPost("{stateId:guid}/act")]
        public Task<IActionResult> Act(Guid stateId) =>
            RunAsync(async () =>
            {
                var userId = _me.GetIdOrThrow();
                var roles = _me.Roles();

                var resource = new WorkflowStateResource
                {
                    WorkflowStateId = stateId,
                    UserId = userId,
                    Roles = roles
                };

                var authz = await _auth.AuthorizeAsync(User, resource, "Step_Act");
                if (!authz.Succeeded)
                {
                    Log.LogWarning("Unauthorized attempt by {UserId} on state {StateId}", userId, stateId);
                    return Forbid();
                }

                // TODO: perform the action
                Log.LogInformation("User {UserId} performed Act on workflow state {StateId}", userId, stateId);
                return Ok(new { message = "Action completed successfully" });
            }, activity: $"POST /api/workflow/{stateId}/act");
    }
}

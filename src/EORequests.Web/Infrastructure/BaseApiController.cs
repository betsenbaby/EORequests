using Microsoft.AspNetCore.Mvc;

namespace EORequests.Web.Infrastructure
{
    [ApiController]
    public abstract class BaseApiController<TSelf> : ControllerBase
    {
        protected readonly ILogger<TSelf> Log;

        protected BaseApiController(ILogger<TSelf> log) => Log = log;

        protected async Task<IActionResult> RunAsync(Func<Task<IActionResult>> action, string? activity = null)
        {
            try
            {
                return await action();
            }
            catch (InvalidOperationException ex)
            {
                return ProblemDetails(401, "Invalid operation", ex, activity);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ProblemDetails(403, "Access denied", ex, activity);
            }
            catch (Exception ex)
            {
                return ProblemDetails(500, "Unexpected server error", ex, activity);
            }
        }

        protected async Task<IActionResult> RunAsync<T>(Func<Task<T>> action, string? activity = null)
        {
            try
            {
                var payload = await action();
                return Ok(payload);
            }
            catch (InvalidOperationException ex)
            {
                return ProblemDetails(401, "Invalid operation", ex, activity);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ProblemDetails(403, "Access denied", ex, activity);
            }
            catch (Exception ex)
            {
                return ProblemDetails(500, "Unexpected server error", ex, activity);
            }
        }

        private IActionResult ProblemDetails(int status, string title, Exception ex, string? activity)
        {
            var traceId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            var path = HttpContext?.Request?.Path.Value ?? "(unknown)";

            Log.LogError(ex,
                "Error during {Activity} [status={Status}, traceId={TraceId}]",
                activity ?? path, status, traceId);

            var pd = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message,
                Instance = path,
                Type = $"https://httpstatuses.com/{status}"
            };

            pd.Extensions["traceId"] = traceId;
            if (!string.IsNullOrWhiteSpace(activity))
                pd.Extensions["activity"] = activity;

            return StatusCode(status, pd);
        }
    }
}

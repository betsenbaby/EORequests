using EORequests.Application.Interfaces;
using Microsoft.Extensions.Logging;


namespace EORequests.Infrastructure.Services
{
    public class LoggingEventDispatcher : IDomainEventDispatcher
    {
        private readonly ILogger<LoggingEventDispatcher> _log;
        public LoggingEventDispatcher(ILogger<LoggingEventDispatcher> log) => _log = log;

        public Task PublishAsync<T>(T evt, CancellationToken ct = default)
        {
            _log.LogInformation("DomainEvent: {@Event}", evt);
            return Task.CompletedTask;
        }
    }
}

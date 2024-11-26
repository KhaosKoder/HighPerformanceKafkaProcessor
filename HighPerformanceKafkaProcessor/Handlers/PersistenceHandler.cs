using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class PersistenceHandler : IMessageHandler<EventMessage>
    {
        private readonly ILogger<PersistenceHandler> _logger;

        public PersistenceHandler(ILogger<PersistenceHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, EventMessage message)
        {
            _logger.LogInformation("Persisting message");
            return Task.CompletedTask;
        }
    }
}

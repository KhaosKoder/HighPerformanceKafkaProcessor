using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class ProcessingHandler : IMessageHandler<EventMessage>
    {
        private readonly ILogger<ProcessingHandler> _logger;

        public ProcessingHandler(ILogger<ProcessingHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, EventMessage message)
        {
            _logger.LogInformation("Processing message");
            return Task.CompletedTask;
        }
    }
}

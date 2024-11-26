using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class PublishAlertHandler : IMessageHandler<EventMessage>
    {
        private readonly ILogger<PublishAlertHandler> _logger;

        public PublishAlertHandler(ILogger<PublishAlertHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, EventMessage message)
        {
            _logger.LogInformation("Processing alerts");
            return Task.CompletedTask;
        }
    }
}

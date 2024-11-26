using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class PreprocessingHandler : IMessageHandler<EventMessage>
    {
        private readonly ILogger<PreprocessingHandler> _logger;

        public PreprocessingHandler(ILogger<PreprocessingHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, EventMessage message)
        {
            _logger.LogInformation("Preprocessing message");
            return Task.CompletedTask;
        }
    }
}

using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Models;

namespace HighPerformanceKafkaProcessor.Handlers
{
    public class PostprocessingHandler : IMessageHandler<EventMessage>
    {
        private readonly ILogger<PostprocessingHandler> _logger;

        public PostprocessingHandler(ILogger<PostprocessingHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(IMessageContext context, EventMessage message)
        {
            _logger.LogInformation("Postprocessing message");
            return Task.CompletedTask;
        }
    }
}

using HighPerformanceKafkaProcessor.Producers;
using KafkaFlow;
using Microsoft.Extensions.Logging;


namespace HighPerformanceKafkaProcessor.Middleware
{
    public class ErrorHandlingMiddleware : IMessageMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly DeadLetterProducer _deadLetterProducer;

        public ErrorHandlingMiddleware(
            ILogger<ErrorHandlingMiddleware> logger,
            DeadLetterProducer deadLetterProducer)
        {
            _logger = logger;
            _deadLetterProducer = deadLetterProducer;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}. Publishing to dead letter topic.",
                    context.ConsumerContext.Topic);

                await _deadLetterProducer.PublishAsync(
                    context.Message.Key,
                    context.Message.Value
                );
            }
        }
    }
}
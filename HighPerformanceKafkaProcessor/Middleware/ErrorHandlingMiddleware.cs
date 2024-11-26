using KafkaFlow;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Producers;

namespace HighPerformanceKafkaProcessor.Middleware
{
    public class ErrorHandlingMiddleware : IMessageMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly DeadLetterProducer _deadLetterProducer;
        private readonly string _topicGroup;

        public ErrorHandlingMiddleware(
            ILogger<ErrorHandlingMiddleware> logger,
            DeadLetterProducer deadLetterProducer,
            string topicGroup)
        {
            _logger = logger;
            _deadLetterProducer = deadLetterProducer;
            _topicGroup = topicGroup;
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing message from topic {Topic} in group {TopicGroup}",
                    context.ConsumerContext.Topic,
                    _topicGroup);

                await _deadLetterProducer.PublishAsync(
                    _topicGroup,
                    context.ConsumerContext.Topic,
                    context.Message.Key?.ToString(),
                    context.Message.Value?.ToString(),
                    ex.Message
                );
            }
        }
    }
}
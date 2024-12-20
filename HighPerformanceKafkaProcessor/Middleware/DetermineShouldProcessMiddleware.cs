
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HighPerformanceKafkaProcessor.Configuration;

namespace HighPerformanceKafkaProcessor.Middleware
{

    /// <summary>
    /// This middleware should determine if a specific Json message should be processed or not.
    /// We DO NOT want to parse the message if it is not absolutely needed, since that would mean
    /// parsing millions of messages per day that we don't want to process. 
    /// </summary>
    public class DetermineShouldProcessMiddleware : IMessageMiddleware
    {
        private readonly ILogger<DetermineShouldProcessMiddleware> _logger;
        private readonly Random _random;

        public DetermineShouldProcessMiddleware(ILogger<DetermineShouldProcessMiddleware> logger)
        {
            _logger = logger;
            _random = new Random();
        }

        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            // Make the processing decision
            bool shouldProcess = _random.NextDouble() <= 0.3;

            // Store the decision in the message context
            context.Items.TryAdd("ShouldProcess", shouldProcess);

            if (!shouldProcess)
            {
                _logger.LogInformation("Message skipped based on processing determination");
                return; // Don't call next middleware
            }

            await next(context);
        }
    }
}


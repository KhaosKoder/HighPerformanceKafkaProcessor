using KafkaFlow;
using Microsoft.Extensions.Logging;

namespace HighPerformanceKafkaProcessor
{
    public class KafkaProcessor
    {
        private readonly IKafkaBus _bus;
        private readonly ILogger<KafkaProcessor> _logger;

        public KafkaProcessor(
            IKafkaBus bus,
            ILogger<KafkaProcessor> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting Kafka processor...");
            await _bus.StartAsync();
            _logger.LogInformation("Kafka processor started successfully");
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping Kafka processor...");
            await _bus.StopAsync();
            _logger.LogInformation("Kafka processor stopped");
        }
    }
}
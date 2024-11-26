using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Configuration;

namespace HighPerformanceKafkaProcessor.Producers
{

    public class DeadLetterProducer :  IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<DeadLetterProducer> _logger;
        private readonly string _deadLetterTopic;

        public DeadLetterProducer(
            IOptions<KafkaOptions> options,
            ILogger<DeadLetterProducer> logger)
        {
            _logger = logger;
            _deadLetterTopic = options.Value.DeadLetterTopic;

            var config = new ProducerConfig
            {
                BootstrapServers = string.Join(",", options.Value.BootstrapServers)
            };

            if (options.Value.UseSsl)
            {
                config.SecurityProtocol = SecurityProtocol.Ssl;
                config.SslCertificateLocation = options.Value.SslCertificateLocation;
                config.SslCaLocation = options.Value.SslCaLocation;
            }

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<TKey, TValue>(TKey key, TValue value)
        {
            try
            {
                var message = new Message<string, string>
                {
                    Key = key?.ToString(),
                    Value = value?.ToString()
                };

                await _producer.ProduceAsync(_deadLetterTopic, message);
                _logger.LogInformation("Message published to dead letter topic {Topic}", _deadLetterTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish to dead letter topic {Topic}", _deadLetterTopic);
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
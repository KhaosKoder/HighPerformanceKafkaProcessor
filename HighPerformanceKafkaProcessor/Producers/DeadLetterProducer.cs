using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using HighPerformanceKafkaProcessor.Configuration;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace HighPerformanceKafkaProcessor.Producers
{

    public class DeadLetterProducer : IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<DeadLetterProducer> _logger;
        private readonly string _deadLetterTopic;

        public DeadLetterProducer(
            IOptions<KafkaOptions> options,
            ILogger<DeadLetterProducer> logger)
        {
            _logger = logger;
            var dlqConfig = options.Value.DeadLetterQueue;
            _deadLetterTopic = dlqConfig.Topic;

            var config = new ProducerConfig
            {
                BootstrapServers = string.Join(",", dlqConfig.BootstrapServers)
            };

            if (dlqConfig.UseSsl)
            {
                config.SecurityProtocol = SecurityProtocol.Ssl;
                config.SslCertificateLocation = dlqConfig.SslCertificateLocation;
                config.SslCaLocation = dlqConfig.SslCaLocation;
            }

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(string topicGroup, string originalTopic, string key, string value, string error)
        {
            try
            {
                var deadLetterMessage = new DeadLetterMessage
                {
                    OriginalTopic = originalTopic,
                    TopicGroup = topicGroup,
                    Key = key,
                    Value = value,
                    ErrorMessage = error,
                    Timestamp = DateTime.UtcNow
                };

                var serializedMessage = JsonConvert.SerializeObject(deadLetterMessage);

                var message = new Message<string, string>
                {
                    Key = key,
                    Value = serializedMessage
                };

                await _producer.ProduceAsync(_deadLetterTopic, message);

                _logger.LogInformation(
                    "Message published to dead letter topic {Topic}. Original topic: {OriginalTopic}, Group: {TopicGroup}",
                    _deadLetterTopic, originalTopic, topicGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish to dead letter topic {Topic}. Original topic: {OriginalTopic}, Group: {TopicGroup}",
                    _deadLetterTopic, originalTopic, topicGroup);
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
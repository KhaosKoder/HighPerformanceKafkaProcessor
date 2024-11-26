namespace HighPerformanceKafkaProcessor.Configuration
{

    public class KafkaOptions
    {
        public string[] Topics { get; set; }
        public string ConsumerGroup { get; set; }
        public string[] BootstrapServers { get; set; }
        public int BufferSize { get; set; } = 100;
        public int WorkersCount { get; set; } = 1;
        public bool UseSsl { get; set; }
        public string SslCertificateLocation { get; set; }
        public string SslCaLocation { get; set; }
        public string DeadLetterTopic { get; set; } = "dead-letter-topic";
        public string SerializerType { get; set; } = "Idiotic"; // or "Simple"
    }
}

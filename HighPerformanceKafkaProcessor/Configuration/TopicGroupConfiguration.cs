namespace HighPerformanceKafkaProcessor.Configuration
{

    public class TopicGroupConfiguration
    {
        public string GroupName { get; set; }
        public bool EnabledOnStartup { get; set; } = true;
        public string[] Topics { get; set; }
        public string ConsumerGroup { get; set; }
        public string[] BootstrapServers { get; set; }
        public int BufferSize { get; set; } = 100;
        public int WorkersCount { get; set; } = 1;
        public string SerializerType { get; set; } = "Idiotic";
        public bool UseSsl { get; set; }
        public string SslCertificateLocation { get; set; }
        public string SslCaLocation { get; set; }
    }
}

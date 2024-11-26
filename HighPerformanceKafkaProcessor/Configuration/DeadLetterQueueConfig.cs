namespace HighPerformanceKafkaProcessor.Configuration
{
    public class DeadLetterQueueConfig
    {
        public string[] BootstrapServers { get; set; }
        public string Topic { get; set; }
        public bool UseSsl { get; set; }
        public string SslCertificateLocation { get; set; }
        public string SslCaLocation { get; set; }
    }

}

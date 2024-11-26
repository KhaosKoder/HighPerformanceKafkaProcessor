namespace HighPerformanceKafkaProcessor.Producers
{
    public class DeadLetterMessage
    {
        public string OriginalTopic { get; set; }
        public string TopicGroup { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
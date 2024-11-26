namespace HighPerformanceKafkaProcessor.Configuration
{

    public class KafkaOptions
    {
        public TopicGroupConfiguration[] TopicGroups { get; set; }
        public DeadLetterQueueConfig DeadLetterQueue { get; set; }
    }
}


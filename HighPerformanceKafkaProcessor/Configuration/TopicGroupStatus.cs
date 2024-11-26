namespace HighPerformanceKafkaProcessor.Configuration
{
    public class TopicGroupStatus
    {
        public string GroupName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRunning { get; set; }
        public DateTime? LastStartTime { get; set; }
        public DateTime? LastStopTime { get; set; }
        public string[] Topics { get; set; }
        public string Error { get; set; }
    }
}


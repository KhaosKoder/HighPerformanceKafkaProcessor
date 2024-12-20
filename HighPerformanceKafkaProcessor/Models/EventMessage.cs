using System;
using Newtonsoft.Json.Linq;

namespace HighPerformanceKafkaProcessor.Models
{
    public class EventMessage : JObject
    {
        public DateTime ProcessingStartTime { get; set; } = DateTime.MinValue;
        public long TicksToProcess { get; set; } = 0;
        public int ActionId { get; set; } = 0;
        public JObject ProcessingAuditTrail { get;set; } = new JObject();

    }
}

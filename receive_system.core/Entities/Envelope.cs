namespace receive_system.core.Entities
{
    public class Envelope
    {
        public string Type { get; set; }
        public string Payload { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public string CorrelationId { get; set; }
        public string Source { get; set; }
        public int RetryCount { get; set; }
    }
}

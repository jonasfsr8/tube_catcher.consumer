namespace receive_system.root.DTOs
{
    public class RabbitMqSettingsDto
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
        public int Port { get; set; }
    }
}

namespace Services
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string Topic { get; set; } = "order-created";
    }
}
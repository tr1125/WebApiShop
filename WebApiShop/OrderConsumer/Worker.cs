using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OrderConsumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            var topic            = _configuration["Kafka:Topic"]            ?? "order-created";
            var groupId          = _configuration["Kafka:GroupId"]          ?? "order-consumer-group";

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId          = groupId,
                AutoOffsetReset  = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            _logger.LogInformation(
                "OrderConsumer starting. BootstrapServers={BS} Topic={Topic} GroupId={GroupId}",
                bootstrapServers, topic, groupId);

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(topic);

            _logger.LogInformation("OrderConsumer subscribed to topic '{Topic}'", topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    _logger.LogInformation(
                        "Order message received — Partition={Partition} Offset={Offset} Key={Key} | Payload: {Value}",
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value,
                        consumeResult.Message.Key,
                        consumeResult.Message.Value);
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka consume error: ErrorCode={Code} Reason={Reason}",
                        ex.Error.Code, ex.Error.Reason);

                    // Back off briefly before retrying on transient errors
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            consumer.Close();
            _logger.LogInformation("OrderConsumer stopped.");
        }
    }
}

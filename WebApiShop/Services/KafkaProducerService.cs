using Confluent.Kafka;
using DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Services
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(
            IProducer<string, string> producer,
            IOptions<KafkaSettings> kafkaSettings,
            ILogger<KafkaProducerService> logger)
        {
            _producer = producer;
            _topic = kafkaSettings.Value.Topic;
            _logger = logger;
        }

        public async Task ProduceOrderCreatedAsync(OrderDTO order)
        {
            var message = new OrderCreatedMessage(
                OrderId: order.OrderId,
                UserId: order.UserId,
                OrderDate: order.OrderDate,
                OrderSum: order.OrderSum,
                Status: order.Status,
                OrderItems: order.OrderItems?
                    .Select(i => new OrderItemMessage(
                        ProductId: i.ProductId,
                        ProductName: i.ProductName,
                        Quantity: i.Quantity,
                        Price: i.Price))
                    .ToList() ?? new List<OrderItemMessage>()
            );

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<string, string>
            {
                Key = order.OrderId.ToString(),
                Value = json
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                var result = await _producer.ProduceAsync(_topic, kafkaMessage, cts.Token);
                _logger.LogInformation(
                    "Kafka message delivered: Topic={Topic} Partition={Partition} Offset={Offset} OrderId={OrderId}",
                    result.Topic, result.Partition.Value, result.Offset.Value, order.OrderId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Kafka produce timed out after 5 s for OrderId={OrderId}. Kafka may be offline.", order.OrderId);
                throw new InvalidOperationException("Kafka broker unreachable (timeout). Order was saved but event was not published.");
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(
                    ex,
                    "Kafka produce failed for OrderId={OrderId}: Error={ErrorCode} Reason={Reason}",
                    order.OrderId, ex.Error.Code, ex.Error.Reason);
                throw;
            }
        }
    }
}

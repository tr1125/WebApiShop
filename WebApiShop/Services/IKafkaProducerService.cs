using DTOs;

namespace Services
{
    public interface IKafkaProducerService
    {
        Task ProduceOrderCreatedAsync(OrderDTO order);
    }
}

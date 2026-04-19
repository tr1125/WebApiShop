using AutoMapper;
using Entities;
using Repositories;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IOrderRepository repository, IMapper mapper, ILogger<OrderService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<OrderDTO> AddOrder(OrderDTO order)
        {
            try
            {
                _logger.LogInformation("AddOrder called: OrderSum={OrderSum}, ItemCount={ItemCount}", order.OrderSum, order.OrderItems?.Count);
                if (order.OrderItems == null || order.OrderItems.Count == 0)
                {
                    _logger.LogWarning("AddOrder rejected: no order items");
                    throw new ArgumentException("Order must contain at least one item.");
                }
                Order order2 = _mapper.Map<OrderDTO, Order>(order);
                Order orderres = await _repository.AddOrder(order2);
                _logger.LogInformation("Order saved to DB: OrderId={OrderId}", orderres.OrderId);
                OrderDTO dto = _mapper.Map<Order, OrderDTO>(orderres);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrder");
                throw;
            }
        }

        public async Task<OrderDTO> GetOrderById(int id)
        {
            _logger.LogInformation("GetOrderById called with id={Id}", id);
            Order order = await _repository.GetOrderById(id);
            if (order == null)
            {
                _logger.LogWarning("Order not found for id={Id}", id);
                throw new KeyNotFoundException($"Order with id {id} was not found.");
            }
            OrderDTO dto = _mapper.Map<Order, OrderDTO>(order);
            return dto;
        }

    }
}

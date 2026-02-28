using AutoMapper;
using Entities;
using Repositories;
using DTOs;
using System;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;
        public OrderService(IOrderRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }


        public async Task<OrderDTO> AddOrder(OrderDTO order)
        {
            try
            {
                Console.WriteLine($"AddOrder called with: OrderId={order.OrderId}, UserId={order.UserId}, OrderSum={order.OrderSum}, ItemCount={order.OrderItems?.Count}");
                
                Order order2 = _mapper.Map<OrderDTO, Order>(order);
                Console.WriteLine($"Mapped to Order entity: OrderId={order2.OrderId}, UserId={order2.UserId}");
                
                Order orderres = await _repository.AddOrder(order2);
                Console.WriteLine($"Order saved to DB: OrderId={orderres.OrderId}");
                
                OrderDTO dto = _mapper.Map<Order, OrderDTO>(orderres);
                return dto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddOrder: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<OrderDTO> GetOrderById(int id)
        {
            Order order = await _repository.GetOrderById(id);
            OrderDTO dto = _mapper.Map<Order, OrderDTO>(order);
            return dto;
        }

        public async Task<List<OrderDTO>> GetAllOrders()
        {
            var orders = await _repository.GetAllOrders();
            return _mapper.Map<List<Order>, List<OrderDTO>>(orders);
        }

        public async Task<List<OrderDTO>> GetOrdersByUserId(int userId)
        {
            var orders = await _repository.GetOrdersByUserId(userId);
            return _mapper.Map<List<Order>, List<OrderDTO>>(orders);
        }

        public async Task<bool> UpdateOrderStatus(int id, string status)
        {
            return await _repository.UpdateOrderStatus(id, status);
        }

    }
}

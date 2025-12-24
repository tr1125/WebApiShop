using AutoMapper;
using Entities;
using Repositories;
using DTOs;

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
            Order order2=_mapper.Map<OrderDTO,Order>(order);
            Order orderres= await _repository.AddOrder(order2);
            OrderDTO dto=_mapper.Map<Order, OrderDTO>(orderres);
            return dto;
        }

        public async Task<OrderDTO> GetOrderById(int id)
        {
            Order order=await _repository.GetOrderById(id);
            OrderDTO dto=_mapper.Map<Order,OrderDTO>(order);
            return dto;
        }

    }
}

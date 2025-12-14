using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }


        public async Task<Order> AddOrder(Order order)
        {
            return await _repository.AddOrder(order);
        }

        public async Task<Order> GetOrderById(int id)
        {
            return await _repository.GetOrderById(id);
        }

    }
}

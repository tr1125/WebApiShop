using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly WebApiShopContext _webApiShopContext;

        public OrderRepository(WebApiShopContext webApiShopContext)
        {
            _webApiShopContext = webApiShopContext;
        }

        public async Task<Order?> GetOrderById(int id)
        {
            Order? order = await _webApiShopContext.Orders.FindAsync(id);
            return order;
        }

        public async Task<Order> AddOrder(Order order)
        {
            await _webApiShopContext.Orders.AddAsync(order);
            await _webApiShopContext.SaveChangesAsync();
            return order;

        }
    }
}

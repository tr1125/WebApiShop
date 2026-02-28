using Entities;
using Microsoft.EntityFrameworkCore;
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
            Order? order = await _webApiShopContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            return order;
        }

        public async Task<Order> AddOrder(Order order)
        {
            await _webApiShopContext.Orders.AddAsync(order);
            await _webApiShopContext.SaveChangesAsync();
            return order;

        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _webApiShopContext.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _webApiShopContext.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatus(int id, string status)
        {
            var order = await _webApiShopContext.Orders.FindAsync(id);
            if (order == null) return false;

            order.Status = status;
            await _webApiShopContext.SaveChangesAsync();
            return true;
        }
    }
}

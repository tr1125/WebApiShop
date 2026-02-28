using DTOs;
using Entities;

namespace Services
{
    public interface IOrderService
    {
        Task<OrderDTO> AddOrder(OrderDTO order);
        Task<OrderDTO> GetOrderById(int id);
        Task<List<OrderDTO>> GetAllOrders();
        Task<List<OrderDTO>> GetOrdersByUserId(int userId);
        Task<bool> UpdateOrderStatus(int id, string status);
    }
}
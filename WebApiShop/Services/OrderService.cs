using AutoMapper;
using Entities;
using System;
using Repositories;
using DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository repository, IMapper mapper, IProductRepository productRepository, ILogger<OrderService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _productRepository = productRepository;
            _logger = logger;
        }


        public async Task<OrderDTO> AddOrder(OrderDTO order)
        {
            try
            {
                _logger.LogInformation("AddOrder called: UserId={UserId}, OrderSum={OrderSum}, ItemCount={ItemCount}", order.UserId, order.OrderSum, order.OrderItems?.Count);

                if (order.OrderItems == null || order.OrderItems.Count == 0)
                {
                    _logger.LogWarning("AddOrder rejected for UserId={UserId}: no order items", order.UserId);
                    throw new ArgumentException("Order must contain at least one item.");
                }

                double calculatedSum = 0;
                foreach (var item in order.OrderItems)
                {
                    var product = await _productRepository.GetProductById(item.ProductId);
                    if (product == null)
                    {
                        _logger.LogWarning("AddOrder rejected: product not found for ProductId={ProductId}", item.ProductId);
                        throw new KeyNotFoundException($"Product with id {item.ProductId} was not found.");
                    }
                    calculatedSum += product.Price * (item.Quantity ?? 1);
                }

                if (order.OrderSum != calculatedSum)
                {
                    _logger.LogWarning("Order sum mismatch for UserId={UserId}. Provided: {ProvidedSum}, Calculated: {CalculatedSum}.", order.UserId, order.OrderSum, calculatedSum);
                    throw new InvalidOperationException($"Order sum mismatch. Provided: {order.OrderSum}, Calculated: {calculatedSum}.");
                }

                Order order2 = _mapper.Map<OrderDTO, Order>(order);
                order2.OrderSum = calculatedSum;

                _logger.LogInformation("Saving order to DB for UserId={UserId}", order2.UserId);
                
                Order orderres = await _repository.AddOrder(order2);
                _logger.LogInformation("Order saved to DB: OrderId={OrderId}", orderres.OrderId);
                
                OrderDTO dto = _mapper.Map<Order, OrderDTO>(orderres);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrder for UserId={UserId}", order?.UserId);
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

        public async Task<List<OrderDTO>> GetAllOrders()
        {
            _logger.LogInformation("GetAllOrders called");
            var orders = await _repository.GetAllOrders();
            _logger.LogInformation("GetAllOrders returned {Count} orders", orders?.Count ?? 0);
            return _mapper.Map<List<Order>, List<OrderDTO>>(orders);
        }

        public async Task<List<OrderDTO>> GetOrdersByUserId(int userId)
        {
            _logger.LogInformation("GetOrdersByUserId called for userId={UserId}", userId);
            var orders = await _repository.GetOrdersByUserId(userId);
            _logger.LogInformation("GetOrdersByUserId returned {Count} orders for userId={UserId}", orders?.Count ?? 0, userId);
            return _mapper.Map<List<Order>, List<OrderDTO>>(orders);
        }

        public async Task<bool> UpdateOrderStatus(int id, string status)
        {
            _logger.LogInformation("UpdateOrderStatus called for orderId={Id}, status={Status}", id, status);
            bool result = await _repository.UpdateOrderStatus(id, status);
            if (!result)
                _logger.LogWarning("UpdateOrderStatus: order not found for id={Id}", id);
            else
                _logger.LogInformation("Order status updated for id={Id}", id);
            return result;
        }

    }
}

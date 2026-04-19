using Xunit;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestWebApiShop.UnitTests
{
    public class OrderServiceUnitTests
    {
        private readonly Mock<IOrderRepository>   _mockOrderRepository;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly OrderService _orderService;

        public OrderServiceUnitTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockLogger          = new Mock<ILogger<OrderService>>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mapper,
                _mockLogger.Object
            );
        }

        #region AddOrder Tests

        /// <summary>
        /// בדיקה: הזמנה עם רשימת פריטים ריקה
        /// Path: UNHAPPY - צריך לזרוק exception
        /// </summary>
        [Fact]
        public async Task AddOrder_WithEmptyOrderItems_ThrowsException()
        {
            // Arrange
            var orderDto = new OrderDTO(
                OrderId:    0,
                OrderDate:  DateOnly.FromDateTime(DateTime.Today),
                OrderSum:   0,
                OrderItems: new List<OrderItemDTO>(),
                Status: "Pending",
                UserId: 1
            );

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _orderService.AddOrder(orderDto)
            );

            _mockOrderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        #endregion

        #region GetOrderById Tests

        /// <summary>
        /// בדיקה: קבלת הזמנה לפי OrderId קיים
        /// Path: HAPPY - צריך להחזיר את ההזמנה
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithExistingId_ReturnsOrder()
        {
            // Arrange
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 200, Status = "Pending" };
            _mockOrderRepository.Setup(x => x.GetOrderById(1)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1,   result.OrderId);
            Assert.Equal(200, result.OrderSum);
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה לפי OrderId שלא קיים
        /// Path: UNHAPPY - צריך לזרוק exception
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithNonExistingId_ThrowsException()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.GetOrderById(999)).ReturnsAsync((Order)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _orderService.GetOrderById(999)
            );
        }

        #endregion
    }
}

using Xunit;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop.UnitTests
{
    public class OrderServiceUnitTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly OrderService _orderService;

        public OrderServiceUnitTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _orderService = new OrderService(_mockOrderRepository.Object, _mapper, _mockProductRepository.Object, _mockLogger.Object);
        }

        #region AddOrder Tests

        [Fact]
        public async Task AddOrder_WithMatchingSum_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var orderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO(1, 2, "Test Product", 100)
            };
            var orderDTO = new OrderDTO(1, dateOnly, 200, orderItems, "Pending", 1);
            
            var product = new Product { ProductId = 1, Price = 100 };
            _mockProductRepository.Setup(x => x.GetProductById(1)).ReturnsAsync(product);

            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 200, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.AddOrder(orderDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.OrderSum);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task AddOrder_WithDifferentSum_LogsWarningAndAddsWithCalculatedSum()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var orderItems = new List<OrderItemDTO>
            {
                new OrderItemDTO(1, 2, "Test Product", 100)
            };
            var orderDTO = new OrderDTO(1, dateOnly, 50, orderItems, "Pending", 1); // 50 instead of 200
            
            var product = new Product { ProductId = 1, Price = 100 };
            _mockProductRepository.Setup(x => x.GetProductById(1)).ReturnsAsync(product);

            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 200, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.AddOrder(orderDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.OrderSum); // Returns the result mapped from DB
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region GetOrderById Tests

        /// <summary>
        /// בדיקה: קבלת הזמנה עם ID תקין
        /// Path: HAPPY - צריך להחזיר את הזמנה
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithValidId_ReturnsOrderDTO()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 150, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.GetOrderById(1)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(150, result.OrderSum);
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה עם ID לא קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.GetOrderById(999)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetOrderById(999);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה עם ID שלילי
        /// Path: UNHAPPY - צריך להחזיר null כי לID שלילי אין הזמנות
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithNegativeId_ReturnsNull()
        {
            // Arrange
            _mockOrderRepository.Setup(x => x.GetOrderById(-1)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetOrderById(-1);

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}

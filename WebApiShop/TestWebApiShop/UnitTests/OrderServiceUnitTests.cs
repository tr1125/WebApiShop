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
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly IMapper _mapper;
        private readonly OrderService _orderService;

        public OrderServiceUnitTests()
        {
            _mockOrderRepository   = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger            = new Mock<ILogger<OrderService>>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            // סדר נכון: IOrderRepository, IMapper, IProductRepository, ILogger
            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mapper,
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        #region AddOrder Tests

        /// <summary>
        /// בדיקה: הזמנה עם סכום תואם למחיר המוצרים
        /// Path: HAPPY - צריך לשמור את ההזמנה ולהחזיר OrderId
        /// </summary>
        [Fact]
        public async Task AddOrder_WithCorrectOrderSum_ReturnsOrder()
        {
            // Arrange
            var product1 = new Product { ProductId = 1, ProductName = "Cola",  Price = 50 };
            var product2 = new Product { ProductId = 2, ProductName = "Chips", Price = 30 };

            var orderDto = new OrderDTO(
                OrderId:    0,
                OrderDate:  DateOnly.FromDateTime(DateTime.Today),
                OrderSum:   130,   // 2*50 + 1*30 = 130 ✓
                OrderItems: new List<OrderItemDTO>
                {
                    new OrderItemDTO(ProductId: 1, Quantity: 2),
                    new OrderItemDTO(ProductId: 2, Quantity: 1)
                },
                Status: "Pending",
                UserId: 1
            );

            _mockProductRepository.Setup(x => x.GetProductById(1)).ReturnsAsync(product1);
            _mockProductRepository.Setup(x => x.GetProductById(2)).ReturnsAsync(product2);
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(new Order { OrderId = 99, OrderSum = 130 });

            // Act
            var result = await _orderService.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99, result.OrderId);
            _mockOrderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
        }

        /// <summary>
        /// בדיקה: הזמנה עם סכום שאינו תואם למחיר המוצרים
        /// Path: UNHAPPY - צריך לכתוב ל-log ולזרוק exception
        /// הערה: כרגע השירות רק מ-log ולא זורק exception - יש לתקן את השירות
        /// </summary>
        [Fact]
        public async Task AddOrder_WithMismatchedOrderSum_LogsAndThrowsException()
        {
            // Arrange
            var product1 = new Product { ProductId = 1, ProductName = "Cola",  Price = 50 };
            var product2 = new Product { ProductId = 2, ProductName = "Chips", Price = 30 };

            var orderDto = new OrderDTO(
                OrderId:    0,
                OrderDate:  DateOnly.FromDateTime(DateTime.Today),
                OrderSum:   9999,  // סכום שגוי! האמיתי הוא 130
                OrderItems: new List<OrderItemDTO>
                {
                    new OrderItemDTO(ProductId: 1, Quantity: 2),
                    new OrderItemDTO(ProductId: 2, Quantity: 1)
                },
                Status: "Pending",
                UserId: 1
            );

            _mockProductRepository.Setup(x => x.GetProductById(1)).ReturnsAsync(product1);
            _mockProductRepository.Setup(x => x.GetProductById(2)).ReturnsAsync(product2);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.AddOrder(orderDto)
            );

            // בדיקה שה-repository לא נקרא כלל
            _mockOrderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>
        /// בדיקה: הזמנה עם רשימת פריטים ריקה
        /// Path: UNHAPPY - צריך לזרוק exception
        /// הערה: כרגע השירות לא בודק זאת - יש לתקן את השירות
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

        /// <summary>
        /// בדיקה: הזמנה עם מוצר שלא קיים ב-DB
        /// Path: UNHAPPY - צריך לזרוק exception
        /// הערה: כרגע השירות מדלג על מוצרים לא קיימים - יש לתקן את השירות
        /// </summary>
        [Fact]
        public async Task AddOrder_WithNonExistentProduct_ThrowsException()
        {
            // Arrange
            var orderDto = new OrderDTO(
                OrderId:    0,
                OrderDate:  DateOnly.FromDateTime(DateTime.Today),
                OrderSum:   50,
                OrderItems: new List<OrderItemDTO>
                {
                    new OrderItemDTO(ProductId: 999, Quantity: 1)
                },
                Status: "Pending",
                UserId: 1
            );

            _mockProductRepository.Setup(x => x.GetProductById(999)).ReturnsAsync((Product)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _orderService.AddOrder(orderDto)
            );

            _mockOrderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>
        /// בדיקה: הזמנה עם פריט אחד בכמות גדולה, סכום נכון
        /// Path: HAPPY - צריך לשמור ולהחזיר OrderId
        /// </summary>
        [Fact]
        public async Task AddOrder_WithLargeQuantitySingleItem_ReturnsOrder()
        {
            // Arrange
            var product = new Product { ProductId = 3, ProductName = "Water", Price = 15 };
            var orderDto = new OrderDTO(
                OrderId:    0,
                OrderDate:  DateOnly.FromDateTime(DateTime.Today),
                OrderSum:   150,   // 10 * 15 = 150 ✓
                OrderItems: new List<OrderItemDTO>
                {
                    new OrderItemDTO(ProductId: 3, Quantity: 10)
                },
                Status: "Pending",
                UserId: 2
            );

            _mockProductRepository.Setup(x => x.GetProductById(3)).ReturnsAsync(product);
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(new Order { OrderId = 55, OrderSum = 150 });

            // Act
            var result = await _orderService.AddOrder(orderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(55, result.OrderId);
            _mockOrderRepository.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
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
        /// הערה: כרגע השירות מחזיר null DTO ולא זורק exception - יש לתקן את השירות
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

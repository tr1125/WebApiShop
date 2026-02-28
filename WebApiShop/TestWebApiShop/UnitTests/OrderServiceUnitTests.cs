using Xunit;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop.UnitTests
{
    public class OrderServiceUnitTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly IMapper _mapper;
        private readonly OrderService _orderService;

        public OrderServiceUnitTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Services.MyMapper>());
            _mapper = config.CreateMapper();
            _orderService = new OrderService(_mockOrderRepository.Object, _mapper);
        }

        #region AddOrder Tests

        /// <summary>
        /// בדיקה: הוספת הזמנה עם דמיון קביע ובטוח טובט
        /// Path: HAPPY - צריך להוסיף את הזמנה בהצלחה
        /// </summary>
        [Fact]
        public async Task AddOrder_WithValidOrder_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var orderDTO = new OrderDTO(1, dateOnly, 100, new List<OrderItemDTO>());
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 100, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.AddOrder(orderDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.OrderSum);
        }

        /// <summary>
        /// בדיקה: הוספת הזמנה עם ף זמנה גבוה (9999)
        /// Path: HAPPY - צריך מהער שההטרטט גם עם רמים בהכה גבוה
        /// </summary>
        [Fact]
        public async Task AddOrder_WithHighOrderSum_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var orderDTO = new OrderDTO(1, dateOnly, 9999, new List<OrderItemDTO>());
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 9999, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.AddOrder(orderDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(9999, result.OrderSum);
        }

        /// <summary>
        /// בדיקה: הוספת הזמנה עם טוח זה (0)
        /// Path: UNHAPPY - צריך להטפל הזמנה אחרי עם אפסנות נרמוך
        /// </summary>
        [Fact]
        public async Task AddOrder_WithZeroOrderSum_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var orderDTO = new OrderDTO(1, dateOnly, 0, new List<OrderItemDTO>());
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 0, OrderDate = dateOnly };
            _mockOrderRepository.Setup(x => x.AddOrder(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            var result = await _orderService.AddOrder(orderDTO);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.OrderSum);
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

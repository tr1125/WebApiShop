using Xunit;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Entities;

namespace TestWebApiShop.IntegrationTests
{
    public class OrderRepositoryIntegrationTests
    {
        private readonly WebApiShopContext _dbContext;
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<WebApiShopContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            _dbContext = new WebApiShopContext(options);
            _orderRepository = new OrderRepository(_dbContext);
        }

        private void CleanUp() => _dbContext?.Dispose();

        #region GetOrderById Tests

        /// <summary>
        /// בדיקה: קבלת הזמנה מדטבש עם ID וטה אחסן
        /// Path: HAPPY - צריך להחזיר את הזמנה אם קיימט
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithValidId_ReturnsOrder()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 100, OrderDate = dateOnly };
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrderId);
            Assert.Equal(100, result.OrderSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה מדטבש עם ID שאט קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetOrderById(999);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה מדטבש עם ID שלילי
        /// Path: UNHAPPY - צריכה שלא קטנטו נה הזמנה
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithNegativeId_ReturnsNull()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 100, OrderDate = dateOnly };
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderById(-1);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region AddOrder Tests

        /// <summary>
        /// בדיקה: הוספת זמנה חדשה בדטבשה בהצלחה
        /// Path: HAPPY - צריך להוסיף את הזמנה באחיד רשמה
        /// </summary>
        [Fact]
        public async Task AddOrder_WithValidOrder_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 150, OrderDate = dateOnly };

            // Act
            var result = await _orderRepository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.OrderId);
            Assert.Equal(150, result.OrderSum);
            var dbOrder = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == 1);
            Assert.NotNull(dbOrder);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת זמנה רע טוח זה (0)
        /// Path: UNHAPPY - צריך להטפל זמנות עם טוח אד פטורט
        /// </summary>
        [Fact]
        public async Task AddOrder_WithZeroSum_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 5, OrderSum = 0, OrderDate = dateOnly };

            // Act
            var result = await _orderRepository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.OrderSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת זמנה עם כניס עדיף רמ (99999)
        /// Path: HAPPY - צריך להטפל טראנטזאקציות אפט אים גדולה
        /// </summary>
        [Fact]
        public async Task AddOrder_WithHighOrderSum_AddsOrderSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, UserId = 1, OrderSum = 99999, OrderDate = dateOnly };

            // Act
            var result = await _orderRepository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99999, result.OrderSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת כמה זמנויות יחדו אחריה בדטבש
        /// Path: HAPPY - צריכה שכל הזמנויות יתשמרו
        /// </summary>
        [Fact]
        public async Task AddOrder_MultipleOrders_AllSavedSuccessfully()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var order1 = new Order { OrderId = 1, UserId = 1, OrderSum = 100, OrderDate = dateOnly };
            var order2 = new Order { OrderId = 2, UserId = 2, OrderSum = 200, OrderDate = dateOnly };

            // Act
            await _orderRepository.AddOrder(order1);
            await _orderRepository.AddOrder(order2);

            // Assert
            var dbOrders = await _dbContext.Orders.ToListAsync();
            Assert.Equal(2, dbOrders.Count);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: דטר: זמנה שהוספנו מט קטנטה ששמרנו
        /// Path: HAPPY - צריך את חמצו אדטרט בדטטיחה לטטיחה
        /// </summary>
        [Fact]
        public async Task AddOrder_RetrievedOrderMatchesInput()
        {
            // Arrange
            var dateOnly = DateOnly.FromDateTime(DateTime.Now);
            var originalOrder = new Order { OrderId = 1, UserId = 3, OrderSum = 250, OrderDate = dateOnly };

            // Act
            await _orderRepository.AddOrder(originalOrder);
            var retrievedOrder = await _orderRepository.GetOrderById(1);

            // Assert
            Assert.NotNull(retrievedOrder);
            Assert.Equal(originalOrder.OrderSum, retrievedOrder.OrderSum);
            Assert.Equal(originalOrder.UserId, retrievedOrder.UserId);
            CleanUp();
        }

        #endregion
    }
}

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

        private User CreateTestUser() => new User
        {
            UserId    = 1,
            UserName  = "test@example.com",
            FirstName = "Israel",
            LastName  = "Israeli",
            Password  = "Pass1!",
            Address   = "Tel Aviv",
            Phone     = "050-0000000"
        };

        private Product CreateTestProduct(int id, double price) => new Product
        {
            ProductId   = id,
            ProductName = $"Product{id}",
            Price       = price,
            CategoryId  = 1,
            IsDeleted   = false
        };

        #region AddOrder Tests

        /// <summary>
        /// בדיקה: הוספת הזמנה תקינה
        /// Path: HAPPY - צריך לשמור את ההזמנה ולהחזיר OrderId
        /// </summary>
        [Fact]
        public async Task AddOrder_WithValidOrder_ReturnsOrderWithId()
        {
            // Arrange
            var user    = CreateTestUser();
            var product = CreateTestProduct(1, 100);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var order = new Order
            {
                UserId     = 1,
                OrderSum   = 100,
                OrderDate  = DateOnly.FromDateTime(DateTime.Today),
                Status     = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = await _orderRepository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.OrderId > 0);
            Assert.Equal(100, result.OrderSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: הוספת הזמנה עם מספר פריטים
        /// Path: HAPPY - צריך לשמור את כל הפריטים
        /// </summary>
        [Fact]
        public async Task AddOrder_WithMultipleItems_SavesAllItems()
        {
            // Arrange
            var user = CreateTestUser();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, 50),
                CreateTestProduct(2, 30)
            );
            await _dbContext.SaveChangesAsync();

            var order = new Order
            {
                UserId     = 1,
                OrderSum   = 130,
                OrderDate  = DateOnly.FromDateTime(DateTime.Today),
                Status     = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2 }, // 2 * 50 = 100
                    new OrderItem { ProductId = 2, Quantity = 1 }  // 1 * 30 = 30
                }
            };

            // Act
            var result = await _orderRepository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.OrderItems.Count);
            CleanUp();
        }

        #endregion

        #region GetOrderById Tests

        /// <summary>
        /// בדיקה: קבלת הזמנה לפי OrderId קיים
        /// Path: HAPPY - צריך להחזיר את ההזמנה עם כל הפריטים
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithExistingId_ReturnsOrder()
        {
            // Arrange
            var user    = CreateTestUser();
            var product = CreateTestProduct(1, 80);
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Products.AddAsync(product);

            var order = new Order
            {
                UserId     = 1,
                OrderSum   = 80,
                OrderDate  = DateOnly.FromDateTime(DateTime.Today),
                Status     = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _orderRepository.GetOrderById(order.OrderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderId, result.OrderId);
            Assert.Equal(80, result.OrderSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: קבלת הזמנה לפי OrderId שלא קיים
        /// Path: UNHAPPY - צריך להחזיר null
        /// </summary>
        [Fact]
        public async Task GetOrderById_WithNonExistingId_ReturnsNull()
        {
            // Act
            var result = await _orderRepository.GetOrderById(999);

            // Assert
            Assert.Null(result);
            CleanUp();
        }

        #endregion

        #region OrderSum Validation Tests

        /// <summary>
        /// בדיקה: סכום ההזמנה תואם לסכום הפריטים
        /// Path: HAPPY - הסכום הנשמר זהה לסכום המחושב מהפריטים
        /// </summary>
        [Fact]
        public async Task AddOrder_OrderSumMatchesItems_HappyPath()
        {
            // Arrange
            var user = CreateTestUser();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Products.AddRangeAsync(
                CreateTestProduct(1, 20),
                CreateTestProduct(2, 15)
            );
            await _dbContext.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 3 }, // 3 * 20 = 60
                new OrderItem { ProductId = 2, Quantity = 2 }  // 2 * 15 = 30
            };

            var order = new Order
            {
                UserId     = 1,
                OrderSum   = 90, // 60 + 30 = 90 ✓
                OrderDate  = DateOnly.FromDateTime(DateTime.Today),
                Status     = "Pending",
                OrderItems = orderItems
            };

            // Act
            var result     = await _orderRepository.AddOrder(order);
            var savedOrder = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstAsync(o => o.OrderId == result.OrderId);

            var calculatedSum = savedOrder.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price);

            // Assert
            Assert.Equal(savedOrder.OrderSum, calculatedSum);
            CleanUp();
        }

        /// <summary>
        /// בדיקה: סכום ההזמנה אינו תואם לסכום הפריטים
        /// Path: UNHAPPY - הסכום הנשמר שגוי, יש לזהות חוסר ההתאמה
        /// </summary>
        [Fact]
        public async Task AddOrder_OrderSumMismatchesItems_UnhappyPath()
        {
            // Arrange
            var user = CreateTestUser();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.Products.AddAsync(CreateTestProduct(1, 50));
            await _dbContext.SaveChangesAsync();

            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 1 } // 1 * 50 = 50
            };

            var order = new Order
            {
                UserId     = 1,
                OrderSum   = 9999, // סכום שגוי! האמיתי הוא 50
                OrderDate  = DateOnly.FromDateTime(DateTime.Today),
                Status     = "Pending",
                OrderItems = orderItems
            };

            // Act
            var result     = await _orderRepository.AddOrder(order);
            var savedOrder = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstAsync(o => o.OrderId == result.OrderId);

            var calculatedSum = savedOrder.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price);

            // Assert - מצב שגוי: הסכום השמור אינו תואם לחישוב
            Assert.NotEqual(savedOrder.OrderSum, calculatedSum);
            CleanUp();
        }

        #endregion
    }
}

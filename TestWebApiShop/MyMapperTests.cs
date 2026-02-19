using Xunit;
using FluentAssertions;
using AutoMapper;
using Services;
using Entities;
using DTOs;

namespace TestWebApiShop
{
    public class MyMapperTests
    {
        private readonly IMapper _mapper;

        public MyMapperTests()
        {
            _mapper = TestHelper.CreateMapper();
        }

        #region Product <-> ProductDTO Tests

        [Fact]
        public void Product_To_ProductDTO_MapsCorrectly()
        {
            // Arrange
            var product = new Product { ProductId = 7, ProductName = "Tea", Price = 4.5, CategoryId = 2, Description = "Herbal tea" };

            // Act
            var dto = _mapper.Map<ProductDTO>(product);

            // Assert
            dto.ProductId.Should().Be(7);
            dto.ProductName.Should().Be("Tea");
            dto.Price.Should().Be(4.5);
            dto.CategoryId.Should().Be(2);
            dto.Description.Should().Be("Herbal tea");
        }

        [Fact]
        public void ProductDTO_To_Product_MapsCorrectly()
        {
            // Arrange
            var dto = new ProductDTO(9, "Coffee", 6.0, 3, "Arabica");

            // Act
            var product = _mapper.Map<Product>(dto);

            // Assert
            product.ProductId.Should().Be(9);
            product.ProductName.Should().Be("Coffee");
            product.Price.Should().Be(6.0);
            product.CategoryId.Should().Be(3);
            product.Description.Should().Be("Arabica");
        }

        [Fact]
        public void Product_To_ProductDTO_WithNullDescription_MapsCorrectly()
        {
            // Arrange
            var product = new Product { ProductId = 1, ProductName = "Juice", Price = 2.5, CategoryId = 1, Description = null };

            // Act
            var dto = _mapper.Map<ProductDTO>(product);

            // Assert
            dto.ProductName.Should().Be("Juice");
            dto.Description.Should().BeNull();
        }

        #endregion

        #region Category <-> CategoryDTO Tests

        [Fact]
        public void Category_To_CategoryDTO_MapsCorrectly()
        {
            // Arrange
            var category = new Category { CetegoryId = 1, CategoryName = "Beverages" };

            // Act
            var dto = _mapper.Map<CategoryDTO>(category);

            // Assert
            dto.CetegoryId.Should().Be(1);
            dto.CategoryName.Should().Be("Beverages");
        }

        [Fact]
        public void CategoryDTO_To_Category_MapsCorrectly()
        {
            // Arrange
            var dto = new CategoryDTO(2, "Snacks");

            // Act
            var category = _mapper.Map<Category>(dto);

            // Assert
            category.CetegoryId.Should().Be(2);
            category.CategoryName.Should().Be("Snacks");
        }

        #endregion

        #region User <-> UserDTO Tests

        [Fact]
        public void User_To_UserDTO_MapsCorrectly()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "john@example.com", FirstName = "John", LastName = "Doe", Password = "SecurePass123" };

            // Act
            var dto = _mapper.Map<UserDTO>(user);

            // Assert
            dto.UserId.Should().Be(1);
            dto.UserName.Should().Be("john@example.com");
            dto.FirstName.Should().Be("John");
            dto.LastName.Should().Be("Doe");
            dto.Password.Should().Be("SecurePass123");
        }

        [Fact]
        public void UserDTO_To_User_MapsCorrectly()
        {
            // Arrange
            var dto = new UserDTO(0, "jane@example.com", "Jane", "Smith", "AnotherPass456");

            // Act
            var user = _mapper.Map<User>(dto);

            // Assert
            user.UserName.Should().Be("jane@example.com");
            user.FirstName.Should().Be("Jane");
            user.LastName.Should().Be("Smith");
            user.Password.Should().Be("AnotherPass456");
        }

        #endregion

        #region Order <-> OrderDTO Tests

        [Fact]
        public void Order_To_OrderDTO_MapsCorrectly()
        {
            // Arrange
            var orderDate = DateOnly.FromDateTime(DateTime.Now);
            var order = new Order { OrderId = 1, OrderDate = orderDate, OrderSum = 99.99 };

            // Act
            var dto = _mapper.Map<OrderDTO>(order);

            // Assert
            dto.OrderId.Should().Be(1);
            dto.OrderDate.Should().Be(orderDate);
            dto.OrderSum.Should().Be(99.99);
        }

        [Fact]
        public void OrderDTO_To_Order_MapsCorrectly()
        {
            // Arrange
            var orderDate = DateOnly.FromDateTime(DateTime.Now);
            var dto = new OrderDTO(2, orderDate, 149.50, new List<OrderItemDTO>());

            // Act
            var order = _mapper.Map<Order>(dto);

            // Assert
            order.OrderId.Should().Be(2);
            order.OrderDate.Should().Be(orderDate);
            order.OrderSum.Should().Be(149.50);
        }

        #endregion

        #region OrderItem <-> OrderItemDTO Tests

        [Fact]
        public void OrderItem_To_OrderItemDTO_MapsCorrectly()
        {
            // Arrange
            var orderItem = new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2 };

            // Act
            var dto = _mapper.Map<OrderItemDTO>(orderItem);

            // Assert
            dto.ProductId.Should().Be(1);
            dto.Quantity.Should().Be(2);
        }

        [Fact]
        public void OrderItemDTO_To_OrderItem_MapsCorrectly()
        {
            // Arrange
            var dto = new OrderItemDTO(1, 3);

            // Act
            var orderItem = _mapper.Map<OrderItem>(dto);

            // Assert
            orderItem.ProductId.Should().Be(1);
            orderItem.Quantity.Should().Be(3);
        }

        #endregion

        #region UserLoginDTO <-> User Tests

        [Fact]
        public void UserLoginDTO_To_User_MapsCorrectly()
        {
            // Arrange
            var loginDTO = new UserLoginDTO("user@example.com", "Password123");

            // Act
            var user = _mapper.Map<User>(loginDTO);

            // Assert
            user.UserName.Should().Be("user@example.com");
            user.Password.Should().Be("Password123");
        }

        [Fact]
        public void User_To_UserLoginDTO_MapsCorrectly()
        {
            // Arrange
            var user = new User { UserId = 1, UserName = "user@example.com", Password = "Password123", FirstName = "John", LastName = "Doe" };

            // Act
            var dto = _mapper.Map<UserLoginDTO>(user);

            // Assert
            dto.UserName.Should().Be("user@example.com");
            dto.Password.Should().Be("Password123");
        }

        #endregion
    }
}

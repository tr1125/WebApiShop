using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using Services;
using Repositories;
using Entities;
using DTOs;

namespace TestWebApiShop
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly IMapper _mapper;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mapper = TestHelper.CreateMapper();
            _service = new OrderService(_mockOrderRepository.Object, _mapper);
        }

        #region AddOrder Tests

        [Fact]
        public async Task AddOrder_WithValidOrder_ReturnsOrderDTO()
        {
            // Arrange
            var orderDTO = new OrderDTO(0, DateOnly.FromDateTime(DateTime.Now), 99.99, new List<OrderItemDTO>());
            var orderEntity = new Order { OrderId = 1, OrderDate = DateOnly.FromDateTime(DateTime.Now), OrderSum = 99.99 };

            _mockOrderRepository.Setup(or => or.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(orderEntity);

            // Act
            var result = await _service.AddOrder(orderDTO);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(1);
            result.OrderSum.Should().Be(99.99);
            _mockOrderRepository.Verify(or => or.AddOrder(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task AddOrder_WithZeroSum_ReturnsOrderDTO()
        {
            // Arrange
            var orderDTO = new OrderDTO(0, DateOnly.FromDateTime(DateTime.Now), 0, new List<OrderItemDTO>());
            var orderEntity = new Order { OrderId = 1, OrderDate = DateOnly.FromDateTime(DateTime.Now), OrderSum = 0 };

            _mockOrderRepository.Setup(or => or.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(orderEntity);

            // Act
            var result = await _service.AddOrder(orderDTO);

            // Assert
            result.Should().NotBeNull();
            result.OrderSum.Should().Be(0);
        }

        [Fact]
        public async Task AddOrder_RepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var orderDTO = new OrderDTO(0, DateOnly.FromDateTime(DateTime.Now), 99.99, new List<OrderItemDTO>());

            _mockOrderRepository.Setup(or => or.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _service.AddOrder(orderDTO);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetOrderById Tests

        [Fact]
        public async Task GetOrderById_WithValidId_ReturnsOrderDTO()
        {
            // Arrange
            var orderId = 1;
            var orderEntity = new Order { OrderId = orderId, OrderDate = DateOnly.FromDateTime(DateTime.Now), OrderSum = 99.99 };

            _mockOrderRepository.Setup(or => or.GetOrderById(orderId))
                .ReturnsAsync(orderEntity);

            // Act
            var result = await _service.GetOrderById(orderId);

            // Assert
            result.Should().NotBeNull();
            result.OrderId.Should().Be(orderId);
            result.OrderSum.Should().Be(99.99);
        }

        [Fact]
        public async Task GetOrderById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var orderId = 999;

            _mockOrderRepository.Setup(or => or.GetOrderById(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _service.GetOrderById(orderId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetOrderById_WithNegativeId_ReturnsNull()
        {
            // Arrange
            var orderId = -1;

            _mockOrderRepository.Setup(or => or.GetOrderById(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _service.GetOrderById(orderId);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}

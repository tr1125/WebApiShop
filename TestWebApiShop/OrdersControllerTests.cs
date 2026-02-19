using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;
using WebApiShop.Controllers;

namespace TestWebApiShop
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrdersController(_mockOrderService.Object);
        }

        #region Get (By ID) Tests

        [Fact]
        public async Task Get_WithValidOrderId_ReturnsOkResult()
        {
            // Arrange
            var orderId = 1;
            var orderDTO = new OrderDTO(orderId, DateOnly.FromDateTime(DateTime.Now), 99.99, new List<OrderItemDTO>());
            _mockOrderService.Setup(s => s.GetOrderById(orderId))
                .ReturnsAsync(orderDTO);

            // Act
            var result = await _controller.Get(orderId);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var okResult = result.Result as OkObjectResult;
            okResult?.Value.Should().Be(orderDTO);
        }

        [Fact]
        public async Task Get_WithInvalidOrderId_ReturnsNotFound()
        {
            // Arrange
            var orderId = 999;
            _mockOrderService.Setup(s => s.GetOrderById(orderId))
                .ReturnsAsync((OrderDTO)null);

            // Act
            var result = await _controller.Get(orderId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Get_WithNegativeOrderId_ReturnsNotFound()
        {
            // Arrange
            var orderId = -1;
            _mockOrderService.Setup(s => s.GetOrderById(orderId))
                .ReturnsAsync((OrderDTO)null);

            // Act
            var result = await _controller.Get(orderId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Get_WithZeroOrderId_ReturnsNotFound()
        {
            // Arrange
            var orderId = 0;
            _mockOrderService.Setup(s => s.GetOrderById(orderId))
                .ReturnsAsync((OrderDTO)null);

            // Act
            var result = await _controller.Get(orderId);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        #endregion

        #region Post (Create Order) Tests

        [Fact]
        public async Task Post_WithValidOrder_ReturnsCreatedAtAction()
        {
            // Arrange
            var orderDate = DateOnly.FromDateTime(DateTime.Now);
            var orderDTO = new OrderDTO(0, orderDate, 99.99, new List<OrderItemDTO>());
            var createdOrder = new OrderDTO(1, orderDate, 99.99, new List<OrderItemDTO>());
            _mockOrderService.Setup(s => s.AddOrder(orderDTO))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.Post(orderDTO);

            // Assert
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result.Result as CreatedAtActionResult;
            createdResult?.ActionName.Should().Be(nameof(OrdersController.Get));
            createdResult?.RouteValues.Should().ContainKey("id");
            createdResult?.RouteValues["id"].Should().Be(1);
        }

        [Fact]
        public async Task Post_WithInvalidOrder_ReturnsBadRequest()
        {
            // Arrange
            var orderDTO = new OrderDTO(0, DateOnly.FromDateTime(DateTime.Now), 0, null);
            _mockOrderService.Setup(s => s.AddOrder(orderDTO))
                .ReturnsAsync((OrderDTO)null);

            // Act
            var result = await _controller.Post(orderDTO);

            // Assert
            result.Result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task Post_WithNegativeOrderSum_ReturnsBadRequest()
        {
            // Arrange
            var orderDTO = new OrderDTO(0, DateOnly.FromDateTime(DateTime.Now), -50.0, new List<OrderItemDTO>());
            _mockOrderService.Setup(s => s.AddOrder(orderDTO))
                .ReturnsAsync((OrderDTO)null);

            // Act
            var result = await _controller.Post(orderDTO);

            // Assert
            result.Result.Should().BeOfType<BadRequestResult>();
        }

        #endregion
    }
}

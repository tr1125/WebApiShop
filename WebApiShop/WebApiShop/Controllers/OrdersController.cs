using AutoMapper;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using Services;
using DTOs;
namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(IOrderService service, ILogger<OrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> Get(int id)
        {
            try
            {
                _logger.LogInformation("GetOrderById called with id={Id}", id);
                OrderDTO order = await _service.GetOrderById(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found for id={Id}", id);
                    return NotFound();
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrderById for id={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpGet]
        public async Task<ActionResult<List<OrderDTO>>> Get()
        {
            try
            {
                _logger.LogInformation("GetAllOrders called");
                List<OrderDTO> orders = await _service.GetAllOrders();
                _logger.LogInformation("GetAllOrders returned {Count} orders", orders.Count);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllOrders");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<OrderDTO>>> GetByUserId(int userId)
        {
            try
            {
                _logger.LogInformation("GetOrdersByUserId called for userId={UserId}", userId);
                List<OrderDTO> orders = await _service.GetOrdersByUserId(userId);
                _logger.LogInformation("GetOrdersByUserId returned {Count} orders for userId={UserId}", orders.Count, userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrdersByUserId for userId={UserId}", userId);
                return StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Post([FromBody] OrderDTO order)
        {
            try
            {
                _logger.LogInformation("AddOrder called for userId={UserId}, orderSum={OrderSum}", order?.UserId, order?.OrderSum);
                if (order == null) return BadRequest("Order cannot be null");
                OrderDTO order2 = await _service.AddOrder(order);
                if (order2 == null) return BadRequest("Failed to create order");
                _logger.LogInformation("Order created successfully with orderId={OrderId}", order2.OrderId);
                return CreatedAtAction(nameof(Get), new { id = order2.OrderId }, order2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrder for userId={UserId}", order?.UserId);
                return StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                _logger.LogInformation("UpdateOrderStatus called for orderId={Id}, status={Status}", id, request?.Status);
                bool success = await _service.UpdateOrderStatus(id, request.Status);
                if (!success)
                {
                    _logger.LogWarning("Order not found for status update, orderId={Id}", id);
                    return NotFound();
                }
                _logger.LogInformation("Order status updated successfully for orderId={Id}", id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateOrderStatus for orderId={Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}

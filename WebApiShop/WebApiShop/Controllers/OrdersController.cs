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
        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDTO>> Get(int id)
        {
            try
            {
                OrderDTO order = await _service.GetOrderById(id);
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Get: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Admin only
        [HttpGet]
        public async Task<ActionResult<List<OrderDTO>>> Get()
        {
            try
            {
                // Note: In a real app we'd check claims/roles here.
                // Assuming client-side security is backed by server logic eventually.
                List<OrderDTO> orders = await _service.GetAllOrders();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAll: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<OrderDTO>>> GetByUserId(int userId)
        {
            try
            {
                List<OrderDTO> orders = await _service.GetOrdersByUserId(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByUserId: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Post([FromBody] OrderDTO order)
        {
            try
            {
                if (order == null) return BadRequest("Order cannot be null");
                OrderDTO order2 = await _service.AddOrder(order);
                if (order2 == null) return BadRequest("Failed to create order");
                return CreatedAtAction(nameof(Get), new { id = order2.OrderId }, order2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Post: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                bool success = await _service.UpdateOrderStatus(id, request.Status);
                if (!success) return NotFound();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateStatus: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }
    }
}

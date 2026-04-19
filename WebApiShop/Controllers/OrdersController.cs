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

        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Post([FromBody] OrderDTO order)
        {
            try
            {
                _logger.LogInformation("AddOrder called");
                if (order == null) return BadRequest("Order cannot be null");
                OrderDTO order2 = await _service.AddOrder(order);
                if (order2 == null) return BadRequest();
                _logger.LogInformation("Order created with id={Id}", order2.OrderId);
                return CreatedAtAction(nameof(Get), new { id = order2.OrderId }, order2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrder");
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}

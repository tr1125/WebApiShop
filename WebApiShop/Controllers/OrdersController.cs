using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<Order>> Get(int id)
        {
            Order order = await _service.GetOrderById(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }


        [HttpPost]
        public async Task<ActionResult<Order>> Post([FromBody] Order order)
        {
            Order order2 = await _service.AddOrder(order);
            if (order2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = order2.OrderId }, order2);
        }

    }
}

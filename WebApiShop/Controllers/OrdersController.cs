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
            OrderDTO order = await _service.GetOrderById(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }


        [HttpPost]
        public async Task<ActionResult<OrderDTO>> Post([FromBody] OrderDTO order)
        {
            OrderDTO order2 = await _service.AddOrder(order);
            if (order2 == null) return BadRequest();
            return CreatedAtAction(nameof(Get), new { id = order2.OrderId }, order2);
        }

    }
}

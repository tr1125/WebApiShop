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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> Get([FromQuery] double? minPrice, [FromQuery] double? maxPrice,
            [FromQuery] List<CategoryDTO>? categories, [FromQuery] string? name)
        {
            
            return await _service.GetProductsByConditions(minPrice, maxPrice, categories, name);
        }
    }
}

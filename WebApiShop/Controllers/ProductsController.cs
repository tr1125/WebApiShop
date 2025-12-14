using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<List<Product>>> Get([FromQuery] double? minPrice,[FromQuery] double? maxPrice,
            [FromQuery] List<Category>? categories, [FromQuery] string? name)
        {
            return await _service.GetProductsByConditions(minPrice,maxPrice,categories,name);
        }
    }
}

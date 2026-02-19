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
        public async Task<List<ProductDTO> /*Items, int TotalCount)*/> Get(int position,
            [FromQuery] double? minPrice, [FromQuery] double? maxPrice,
            [FromQuery] string? name,
            [FromQuery]string? desc, [FromQuery] int?[]categoryIds)

        {
            int skip = 10;
            (List<ProductDTO> product, int total) = await _service.GetProductsByConditions(position, skip,minPrice, maxPrice, name, desc, categoryIds);
            //return (product, total);
            return product;
        }
    }
}

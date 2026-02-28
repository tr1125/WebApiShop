using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using DTOs;
namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<List<CategoryDTO>> Get()
        { 
            return await _service.GetAllCategories();
        }

    }
}

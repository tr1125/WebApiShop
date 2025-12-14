using Entities;
using Services;
using Repositories;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<List<Category>> Get()
        {
            return await _service.GetAllCategories();
        }

    }
}

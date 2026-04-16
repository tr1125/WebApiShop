using AutoMapper;
using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Extensions.Logging;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(ICategoryRepository repository, IMapper mapper, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<List<CategoryDTO>> GetAllCategories()
        {
            _logger.LogInformation("GetAllCategories called");
            List<Category> category = await _repository.GetAllCategories();
            if (category == null || category.Count == 0)
                _logger.LogWarning("GetAllCategories returned no categories");
            else
                _logger.LogInformation("GetAllCategories found {Count} categories", category.Count);
            List<CategoryDTO> dto = _mapper.Map<List<Category>, List<CategoryDTO>>(category);
            return dto;
        }

        public async Task<CategoryDTO> AddCategory(CategoryDTO category)
        {
            _logger.LogInformation("AddCategory called with name={Name}", category?.CategoryName);
            Category category2 = _mapper.Map<CategoryDTO, Category>(category);
            Category category3 = await _repository.AddCategory(category2);
            if (category3 == null)
            {
                _logger.LogWarning("AddCategory failed for name={Name}", category?.CategoryName);
                return null;
            }
            _logger.LogInformation("Category added with id={Id}", category3.CetegoryId);
            CategoryDTO category4 = _mapper.Map<Category, CategoryDTO>(category3);
            return category4;
        }

        
    }
}

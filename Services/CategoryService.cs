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

        
    }
}

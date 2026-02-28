using AutoMapper;
using Entities;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;
        public CategoryService(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<List<CategoryDTO>> GetAllCategories()
        {

            List<Category> category = await _repository.GetAllCategories();
            List<CategoryDTO> dto = _mapper.Map<List<Category>, List<CategoryDTO>>(category);
            return dto;
        }

        
    }
}

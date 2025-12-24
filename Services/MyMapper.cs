using AutoMapper;
using WebApiShop.Controllers;
using Entities;
using DTOs;


namespace WebApiShop
{
    public class MyMapper: Profile
    {
        public MyMapper() {
            CreateMap<Product, ProductDTO>();
            CreateMap<Category, CategoryDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<Order, OrderDTO>();
        }
        
    }
}

using AutoMapper;
using Entities;
using DTOs;


namespace Services
{
    public class MyMapper: Profile
    {
        public MyMapper() {
            CreateMap<Product, ProductDTO>();
            CreateMap<ProductDTO, Product>();
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryDTO, Category>();
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address != null ? src.Address.Trim() : null))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Trim() : null));
            CreateMap<UserDTO, User>();
            CreateMap<UserLoginDTO, User>();
            CreateMap<User, UserLoginDTO>();
            CreateMap<Order, OrderDTO>();
            CreateMap<OrderDTO, Order>()
                .ForMember(dest => dest.OrderId, opt => opt.Ignore());
            CreateMap<OrderItem, OrderItemDTO>();
            CreateMap<OrderItemDTO, OrderItem>()
                .ForMember(dest => dest.OrderId, opt => opt.Ignore());
        }
        
    }
}

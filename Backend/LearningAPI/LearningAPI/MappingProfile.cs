using AutoMapper;
using LearningAPI.Models;

namespace LearningAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<User, UserBasicDTO>();
        }
    }
}

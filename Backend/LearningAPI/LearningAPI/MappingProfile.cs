using AutoMapper;
using LearningAPI.Models;
using LearningAPI.Models.Latiff;

namespace LearningAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<User, UserBasicDTO>();
      			CreateMap<Donation, DonationDTO>();
            CreateMap<UserRole, UserRoleDTO>();
        }

    }
}

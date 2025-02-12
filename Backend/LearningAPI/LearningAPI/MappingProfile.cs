using AutoMapper;
using LearningAPI.Models;
using LearningAPI.Models.Joseph;
using LearningAPI.Models.Latiff;

namespace LearningAPI
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>().ReverseMap();
            //.ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            //.ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            //.ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants));
            
            CreateMap<User, UserDTO>();
            CreateMap<User, UserBasicDTO>();
      		CreateMap<Donation, DonationDTO>();
            CreateMap<UserRole, UserRoleDTO>();
            CreateMap<Feedback, FeedbackDTO>();
            
            CreateMap<Variant, VariantDTO>().ReverseMap();
        }

    }
}

using AutoMapper;
using LearningAPI.DTOs;
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
            
            CreateMap<User, UserDTO>();
            CreateMap<User, UserBasicDTO>();
      		CreateMap<Donation, DonationDTO>();
            CreateMap<UserRole, UserRoleDTO>();
            CreateMap<Feedback, FeedbackDTO>();
            
            CreateMap<Variant, VariantDTO>().ReverseMap();
            CreateMap<Review, ReviewDTO>();
        }

    }
}

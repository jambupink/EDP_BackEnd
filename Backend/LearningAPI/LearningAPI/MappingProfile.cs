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
            CreateMap<Tutorial, TutorialDTO>();
            CreateMap<User, UserDTO>();
            CreateMap<User, UserBasicDTO>();
            CreateMap<UserRole, UserRoleDTO>();
            CreateMap<Feedback, FeedbackDTO>();
        }
    }
}

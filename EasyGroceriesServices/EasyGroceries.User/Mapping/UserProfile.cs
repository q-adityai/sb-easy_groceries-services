using AutoMapper;
using EasyGroceries.User.Dto;

namespace EasyGroceries.User.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserDto, Model.Entities.User>()
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dst => dst.IsAdmin, opt => opt.MapFrom(src => false));

        CreateMap<Model.Entities.User, UserDto>();
    }
}
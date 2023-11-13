using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;

namespace EasyGroceries.User.Mapping;

public class UserInactiveProfile : Profile
{
    public UserInactiveProfile()
    {
        CreateMap<Model.Entities.User, UserInactiveEvent>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => EventType.UserInactive))
            .ForMember(dst => dst.CorrelationId, opt => opt.Ignore());
    }
}
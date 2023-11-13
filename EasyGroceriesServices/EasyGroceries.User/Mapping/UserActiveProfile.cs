using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;

namespace EasyGroceries.User.Mapping;

public class UserActiveProfile : Profile
{
    public UserActiveProfile()
    {
        CreateMap<Model.Entities.User, UserActiveEvent>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => EventType.UserActive))
            .ForMember(dst => dst.CorrelationId, opt => opt.Ignore());
    }
}
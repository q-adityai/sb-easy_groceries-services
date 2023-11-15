using AutoMapper;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Order.Model.Entities;

namespace EasyGroceries.Order.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedEvent, User>();
        CreateMap<UserUpdatedEvent, User>();
    }
}
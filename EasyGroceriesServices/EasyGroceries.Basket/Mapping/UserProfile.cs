using AutoMapper;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Messaging.Events;

namespace EasyGroceries.Basket.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreatedEvent, User>();
    }
}
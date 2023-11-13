using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using Address = EasyGroceries.Common.Entities.Address;

namespace EasyGroceries.User.Mapping;

public class UserCreatedProfile : Profile
{
    public UserCreatedProfile()
    {
        CreateMap<Model.Entities.User, UserCreatedEvent>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => EventType.UserCreated))
            .ForMember(dst => dst.CorrelationId, opt => opt.Ignore())
            .ForMember(dst => dst.DefaultBillingAddress,
                opt => opt.MapFrom(src =>
                    src.BillingAddresses.Count > 0 ? src.BillingAddresses.Find(ba => ba.IsDefault)!.Address : null))
            .ForMember(dst => dst.DefaultDeliveryAddress,
                opt => opt.MapFrom(src =>
                    src.DeliveryAddresses.Count > 0 ? src.DeliveryAddresses.Find(da => da.IsDefault)!.Address : null));

        CreateMap<Address, Common.Messaging.Events.Address>();
    }
}
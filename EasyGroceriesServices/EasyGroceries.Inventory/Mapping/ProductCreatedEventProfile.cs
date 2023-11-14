using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Mapping;

public class ProductCreatedEventProfile : Profile
{
    public ProductCreatedEventProfile()
    {
        CreateMap<Product, ProductCreatedEvent>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => EventType.ProductCreated))
            .ForMember(dst => dst.CorrelationId, opt => opt.Ignore())
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => src.Sku.Clone()))
            .ForMember(dst => dst.Price, opt => opt.MapFrom(src => src.Price.Clone()));
    }
}
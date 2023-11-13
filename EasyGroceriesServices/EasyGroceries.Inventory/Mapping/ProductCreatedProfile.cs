using AutoMapper;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Mapping;

public class ProductCreatedProfile : Profile
{
    public ProductCreatedProfile()
    {
        CreateMap<Product, ProductCreatedEvent>()
            .ForMember(dst => dst.Type, opt => opt.MapFrom(src => EventType.ProductCreated))
            .ForMember(dst => dst.CorrelationId, opt => opt.Ignore())
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => src.Sku.Code))
            .ForMember(dst => dst.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dst => dst.IncludeInDelivery,
                opt => opt.MapFrom(src => src.Category != ProductCategory.PromotionCoupon))
            .ForMember(dst => dst.DiscountApplicable,
                opt => opt.MapFrom(src => src.Category != ProductCategory.PromotionCoupon));
    }
}
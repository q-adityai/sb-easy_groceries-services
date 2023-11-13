using AutoMapper;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Messaging.Events;

namespace EasyGroceries.Basket.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductCreatedEvent, Product>()
            .ForMember(dst => dst.DiscountedPrice, opt => opt.MapFrom(src => src.Price.Clone()))
            .ForMember(dst => dst.DiscountInMinorUnits, opt => opt.MapFrom(src => 0));
    }
}
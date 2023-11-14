using System;
using AutoMapper;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => src.Sku.Code));

        CreateMap<ProductDto, Product>()
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.DiscountApplicable,
                opt => opt.MapFrom(src =>
                    src.Category != ProductCategory.PromotionCoupon))
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => Sku.Generate()))
            .ForMember(dst => dst.ValidFrom, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dst => dst.ValidTo, opt => opt.MapFrom(src => DateTimeOffset.MaxValue));
    }
}
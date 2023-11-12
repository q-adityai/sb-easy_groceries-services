using AutoMapper;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => src.Sku.Code));
    }
}
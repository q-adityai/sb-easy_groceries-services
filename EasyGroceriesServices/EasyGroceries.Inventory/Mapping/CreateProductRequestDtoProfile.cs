using System;
using AutoMapper;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Extensions;
using EasyGroceries.Common.Utils;
using EasyGroceries.Inventory.Dto;
using EasyGroceries.Inventory.Model.Entities;

namespace EasyGroceries.Inventory.Mapping;

public class CreateProductRequestDtoProfile : Profile
{
    public CreateProductRequestDtoProfile()
    {
        CreateMap<CreateProductRequestDto, Product>()
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.Sku, opt => opt.MapFrom(src => new Sku{ Code = SkuCodeGenerator.Generate() }))
            .ForMember(dst => dst.Price, opt => opt.MapFrom(src => src.Price.Clone()));
    }
}
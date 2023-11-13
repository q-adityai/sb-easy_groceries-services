using AutoMapper;
using EasyGroceries.Basket.Model.Entities;
using EasyGroceries.Common.Messaging.Events;

namespace EasyGroceries.Basket.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductCreatedEvent, Product>();
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Order.Model.Entities;

public class OrderItem
{
    [Key] public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IncludeInDelivery { get; set; }
    public Money Price { get; set; }
    public Money DiscountedPrice { get; set; }
    public long DiscountPercentInMinorUnits { get; set; }
    public long Quantity { get; set; }
    
    public string ProductId { get; set; } = null!;
    public string OrderId { get; set; } = null!;

    public OrderItem()
    {
        Id = $"{Constants.OrderPrefix}{Constants.ProductPrefix}{Guid.NewGuid()}";
    }
}
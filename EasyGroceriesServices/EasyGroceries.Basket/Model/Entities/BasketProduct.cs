using System;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace EasyGroceries.Basket.Model.Entities;

public class BasketProduct
{
    [Key] public string Id { get; set; }
    public string ProductId { get; set; } = null!;

    public string SkuCode { get; set; } = null!;

    public ProductCategory Category { get; set; }

    public bool IncludeInDelivery { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Money Price { get; set; } = null!;
    public Money DiscountedPrice { get; set; } = null!;

    public int DiscountPercentInMinorUnits { get; set; }

    public bool DiscountApplicable { get; set; }
    public long Quantity { get; set; }
    public string BasketId { get; set; } = null!;

    public BasketProduct()
    {
        Id = $"{Constants.BasketPrefix}{Constants.ProductPrefix}{Guid.NewGuid()}";
    }
}
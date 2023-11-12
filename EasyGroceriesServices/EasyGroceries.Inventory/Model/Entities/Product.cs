using System;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Inventory.Model.Entities;

public class Product
{
    public string Id { get; set; } = $"{Constants.ProductPrefix}{Guid.NewGuid()}";
    public Sku Sku { get; set; }
    public ProductCategory Category { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Money Price { get; set; }
    public long StockQuantity { get; set; }
    public bool DiscountApplicable { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset ValidTo { get; set; }
}
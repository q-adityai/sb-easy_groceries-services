using System;
using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Inventory.Model.Entities;

public class Product
{
    [Key]public string Id { get; set; }
    public Sku Sku { get; set; } = null!;
    public ProductCategory Category { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public bool DiscountApplicable { get; set; }
    public long StockQuantity { get; set; }
    public Product()
    {
        Id = $"{Constants.ProductPrefix}{Guid.NewGuid()}";
    }
}
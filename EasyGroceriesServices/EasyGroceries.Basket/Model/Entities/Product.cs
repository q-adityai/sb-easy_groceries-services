using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Basket.Model.Entities;

public class Product
{
    [Key] public string Id { get; set; } = null!;
    public Sku Sku { get; set; } = null!;
    public ProductCategory Category { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public bool DiscountApplicable { get; set; }
    public long StockQuantity { get; set; }
    public Money DiscountedPrice { get; set; } = null!;
    public int AppliedDiscountPercentInMinorUnits { get; set; }
}
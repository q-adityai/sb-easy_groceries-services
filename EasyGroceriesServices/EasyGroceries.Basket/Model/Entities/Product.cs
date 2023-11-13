using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Basket.Model.Entities;

public class Product
{
    [Required] public string Id { get; set; } = null!;

    [Required] public string Sku { get; set; } = null!;

    [Required] public string CategoryName { get; set; } = null!;

    [Required] public bool IncludeInDelivery { get; set; }

    [Required] public string Name { get; set; } = null!;

    [Required] public string Description { get; set; } = null!;

    [Required] public Money Price { get; set; } = null!;
    [Required] public Money DiscountedPrice { get; set; } = null!;

    [Required] public int DiscountInMinorUnits { get; set; } = 0;

    [Required] public bool DiscountApplicable { get; set; }
}
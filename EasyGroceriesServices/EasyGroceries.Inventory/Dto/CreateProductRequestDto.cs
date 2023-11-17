using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Attributes;
using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Inventory.Dto;

public class CreateProductRequestDto
{
    [RequiredEnum] public ProductCategory Category { get; set; }

    [Required] [StringLength(100)] public string Name { get; set; } = null!;

    [Required] [StringLength(500)] public string Description { get; set; } = null!;

    [Required] public Money Price { get; set; } = null!;
    
    [Required] public bool DiscountApplicable { get; set; }

    [Required] [Range(1, long.MaxValue)] public long StockQuantity { get; set; }
}
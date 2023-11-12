using EasyGroceries.Common.Entities;

namespace EasyGroceries.Inventory.Dto;

public class ProductDto
{
    public string Id { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public CategoryDto Category { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public long StockQuantity { get; set; } = 0;
}
namespace EasyGroceries.Inventory.Dto;

public class CategoryDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = false;
    public bool IncludeInDelivery { get; set; } = false;
}
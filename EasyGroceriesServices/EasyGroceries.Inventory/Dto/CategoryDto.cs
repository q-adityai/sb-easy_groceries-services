using System.ComponentModel.DataAnnotations;

namespace EasyGroceries.Inventory.Dto;

public class CategoryDto
{
    public string? Id { get; set; }
    
    [Required]
    public string Name { get; set; } = null!;
    
    public bool IsActive { get; set; }
    public bool IncludeInDelivery { get; set; }
}
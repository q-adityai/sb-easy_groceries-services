using System.ComponentModel.DataAnnotations;

namespace EasyGroceries.Basket.Dto;

public class BasketProductRequestDto
{
    public string? BasketId { get; set; }
    [Required]
    public string UserId { get; set; } = null!;
    [Required]
    public string ProductId { get; set; } = null!;
    [Required]
    [Range(1, long.MaxValue)]
    public long Quantity { get; set; } = 0;
}
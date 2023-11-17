using EasyGroceries.Common.Entities;

namespace EasyGroceries.Order.Dto;

public class OrderItemDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Money Price { get; set; }
    public Money DiscountedPrice { get; set; }
    public long DiscountPercentInMinorUnits { get; set; }
    public long Quantity { get; set; }
}
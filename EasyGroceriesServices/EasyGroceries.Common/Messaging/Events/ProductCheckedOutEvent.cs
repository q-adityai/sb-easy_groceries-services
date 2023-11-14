using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class ProductCheckedOutEvent : BaseEvent
{
    public string BasketId { get; set; }
    public string UserId { get; set; }
    public string ProductId { get; set; }
    public string Name { get; set; }
    public long Quantity { get; set; }
    
    public Money Price { get; set; } = null!;
    public Money DiscountedPrice { get; set; } = null!;

    public int DiscountPercentInMinorUnits { get; set; }
    
    public bool IncludeInDelivery { get; set; }
    public ProductCheckedOutEvent()
    {
        Type = EventType.ProductCheckedOut;
    }
}
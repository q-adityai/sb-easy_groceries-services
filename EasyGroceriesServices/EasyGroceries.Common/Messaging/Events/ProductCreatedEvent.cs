using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class ProductCreatedEvent : BaseEvent
{
    public ProductCreatedEvent()
    {
        Type = EventType.ProductCreated;
    }

    public string Id { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public bool IncludeInDelivery { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Money Price { get; set; } = null!;
    public long StockQuantity { get; set; }
    public bool DiscountApplicable { get; set; }
}
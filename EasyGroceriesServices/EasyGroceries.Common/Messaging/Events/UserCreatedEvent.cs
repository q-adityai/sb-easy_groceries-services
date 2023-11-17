using EasyGroceries.Common.Entities;
using EasyGroceries.Common.Enums;
namespace EasyGroceries.Common.Messaging.Events;

public sealed class UserCreatedEvent : BaseEvent
{
    public UserCreatedEvent()
    {
        Type = EventType.UserCreated;
    }

    public string Id { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DefaultAddress? DefaultBillingAddress { get; set; }
    public DefaultAddress? DefaultDeliveryAddress { get; set; }
}
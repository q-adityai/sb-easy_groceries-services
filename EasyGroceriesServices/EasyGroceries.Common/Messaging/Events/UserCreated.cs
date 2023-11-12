using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public sealed class UserCreated : BaseEvent
{
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? DefaultBillingAddress { get; set; }
    public Address? DefaultDeliveryAddress { get; set; }
    public UserCreated()
    {
        Type = EventType.UserCreated;
    }
}
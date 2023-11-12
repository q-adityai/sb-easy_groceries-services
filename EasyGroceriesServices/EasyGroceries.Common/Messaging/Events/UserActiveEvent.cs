using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class UserActiveEvent : BaseEvent
{
    public string Id { get; set; } = null!;
    public UserActiveEvent()
    {
        Type = EventType.UserActive;
    }
}
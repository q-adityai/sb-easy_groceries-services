using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class UserActiveEvent : BaseEvent
{
    public UserActiveEvent()
    {
        Type = EventType.UserActive;
    }

    public string Id { get; set; } = null!;
}
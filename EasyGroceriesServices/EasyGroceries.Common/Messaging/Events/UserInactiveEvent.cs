using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class UserInactiveEvent : BaseEvent
{
    public string Id { get; set; } = null!;

    public UserInactiveEvent()
    {
        Type = EventType.UserInactive;
    }
}
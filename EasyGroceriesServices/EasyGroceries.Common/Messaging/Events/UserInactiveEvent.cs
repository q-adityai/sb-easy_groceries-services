using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Events;

public class UserInactiveEvent : BaseEvent
{
    public UserInactiveEvent()
    {
        Type = EventType.UserInactive;
    }

    public string Id { get; set; } = null!;
}
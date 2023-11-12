using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Events;
using EasyGroceries.Common.Messaging.Interfaces;

namespace EasyGroceries.Common.Extensions;

public static class BaseEventExtensions
{
    public static string GetTopicName(this IEvent baseEvent)
    {
        return baseEvent.Type switch
        {
            EventType.UserCreated => "easy-groceries-user",
            EventType.UserActive => "easy-groceries-user",
            EventType.UserInactive => "easy-groceries-user",
            EventType.UserDeleted => "easy-groceries-user",
            _ => string.Empty
        };
    }
}
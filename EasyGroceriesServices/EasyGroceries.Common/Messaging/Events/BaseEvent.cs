using EasyGroceries.Common.Enums;
using EasyGroceries.Common.Messaging.Interfaces;

namespace EasyGroceries.Common.Messaging.Events;

public abstract class BaseEvent : IEvent
{
    public EventType Type { get; set; }
    public string? CorrelationId { get; set; }
}
using EasyGroceries.Common.Enums;

namespace EasyGroceries.Common.Messaging.Interfaces;

public interface IEvent
{
    public EventType Type { get; set; }
    public string? CorrelationId { get; set; }
}
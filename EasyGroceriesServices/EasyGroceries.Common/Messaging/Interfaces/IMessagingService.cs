using System.Threading.Tasks;

namespace EasyGroceries.Common.Messaging.Interfaces;

public interface IMessagingService
{
    Task EmitEventAsync(IEvent baseEvent);
    Task EmitEventsAsync<T>(List<T> baseEvents) where T : IEvent;
}
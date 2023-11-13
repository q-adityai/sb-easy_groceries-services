using System.Threading.Tasks;

namespace EasyGroceries.Common.Messaging.Interfaces;

public interface IMessagingService
{
    Task EmitEvent(IEvent baseEvent);
}
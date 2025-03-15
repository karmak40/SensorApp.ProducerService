using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Events;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public interface IEventRepository
    {
        Task SaveEventAsync(IEvent @event);
        Task SaveSensorDataAsync(SensorData sensorData);
    }
}

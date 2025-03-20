using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public interface IEventRepository
    {
        Task SaveSensorDataAsync(SensorData sensorData);
        Task<bool> HasNotSentMessages();
        Task<List<SensorEventEntity>> GetUnsendMessages();
        Task UpdateEventStatusAsync(Guid eventId, EventStatus newStatus);
    }
}

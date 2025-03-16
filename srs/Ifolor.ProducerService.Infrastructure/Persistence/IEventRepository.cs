using IfolorProducerService.Application.Services;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public interface IEventRepository
    {
        Task SaveSensorDataAsync(SensorData sensorData);
        Task<bool> HasNotSentMessages();
    }
}

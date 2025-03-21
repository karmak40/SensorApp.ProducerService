using IfolorProducerService.Application.Services;

namespace Ifolor.ProducerService.Core.Services
{
    public interface IMessageProducer
    {
        Task SendMessage(SensorData message);
    }
}

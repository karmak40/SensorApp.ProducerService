using IfolorProducerService.Application.Services;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public interface IMessageProducer
    {
        Task SendMessage(SensorData message);
    }
}

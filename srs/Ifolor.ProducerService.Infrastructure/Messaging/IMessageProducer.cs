using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public interface IMessageProducer
    {
        Task SendMessage(RabbitMQConfig queueName, string message);
    }
}

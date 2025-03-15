
using RabbitMQ.Client;
using System.Text;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public class RabbitMQProducer : IMessageProducer
    {
        public async Task SendMessage()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            const string mes = "Hello World!";
            var body = Encoding.UTF8.GetBytes(mes);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {mes}");

        }
    }
}
    
using Ifolor.ProducerService.Infrastructure.Messaging;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using RabbitMQ.Client.Exceptions;

namespace Ifolor.ProducerService.Tests
{
    public class SendServiceTests
    {
        private readonly Mock<IMessageProducer> _mockMessageProducer;
        private readonly Mock<ILogger<SendService>> _mockLogger;
        private readonly RabbitMQConfig _rabbitMQConfig;
        private readonly SendService _sendService;

        public SendServiceTests()
        {
            _mockMessageProducer = new Mock<IMessageProducer>();
            _mockLogger = new Mock<ILogger<SendService>>();
            _rabbitMQConfig = new RabbitMQConfig { HostName = "localhost" };

            var mockOptions = new Mock<IOptions<RabbitMQConfig>>();
            mockOptions.Setup(o => o.Value).Returns(_rabbitMQConfig);

            _sendService = new SendService(_mockMessageProducer.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SendToMessageBroker_SuccessfullySendsMessage()
        {
            // Arrange
            var sensorData = new SensorData { EventId = Guid.NewGuid(), SensorId = "sensor-1" };
            _mockMessageProducer
                .Setup(m => m.SendMessage(sensorData))
                .Returns(Task.CompletedTask);

            // Act
            await _sendService.SendToMessageBroker(sensorData);

            // Assert
            _mockMessageProducer.Verify(m => m.SendMessage(sensorData), Times.Once);
        }


        [Fact]
        public async Task SendToMessageBroker_RetriesOnBrokerUnreachableException()
        {
            // Arrange
            var sensorData = new SensorData { EventId = Guid.NewGuid(), SensorId = "sensor-1" };
            _mockMessageProducer
                .SetupSequence(m => m.SendMessage(sensorData))
                .Throws(new BrokerUnreachableException(new Exception("Broker unreachable")))
                .Throws(new BrokerUnreachableException(new Exception("Broker unreachable")))
                .Returns(Task.CompletedTask);

            // Act
            await _sendService.SendToMessageBroker(sensorData);

            // Assert
            _mockMessageProducer.Verify(m => m.SendMessage(sensorData), Times.Exactly(3));
        }

        [Fact]
        public async Task SendToMessageBroker_PropagatesOperationCanceledException()
        {
            // Arrange
            var sensorData = new SensorData { EventId = Guid.NewGuid(), SensorId = "sensor-1" };
            _mockMessageProducer
                .Setup(m => m.SendMessage(sensorData))
                .Throws(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _sendService.SendToMessageBroker(sensorData));
        }

        [Fact]
        public async Task SendToMessageBroker_HandlesGenericException()
        {
            // Arrange
            var sensorData = new SensorData { EventId = Guid.NewGuid(), SensorId = "sensor-1" };
            var exception = new Exception("Unexpected error");
            _mockMessageProducer
                .Setup(m => m.SendMessage(sensorData))
                .Throws(exception);

            // Act & Assert
            await _sendService.SendToMessageBroker(sensorData);
            Assert.Equal(EventStatus.NotSend, sensorData.EventStatus);
        }
    }
}
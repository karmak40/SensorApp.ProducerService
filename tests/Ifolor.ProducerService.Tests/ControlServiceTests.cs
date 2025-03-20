using AutoMapper;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Generator;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Generator;
using IfolorProducerService.Core.Models;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Ifolor.ProducerService.Tests
{
    public class ControlServiceTests
    {
        private readonly Mock<ISendService> _sendServiceMock;
        private readonly Mock<IResendService> _resendServiceMock;
        private readonly Mock<ISensorService> _sensorServiceMock;
        private readonly Mock<IMessageProducer> _messageProducerMock;
        private readonly ISensorDataGenerator  _sensorDataGenerator;
        private readonly Mock<IOptions<ProducerPolicy>> _producerPolicyOptionsMock;
        private readonly Mock<ILogger<ControlService>> _loggerMock;
        private readonly Mock<IEventRepository> _eventRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly ControlService _controlService;

        public ControlServiceTests()
        {
            _sendServiceMock = new Mock<ISendService>();
            _resendServiceMock = new Mock<IResendService>();
            _sensorServiceMock = new Mock<ISensorService>();
            _messageProducerMock = new Mock<IMessageProducer>();
            _sensorDataGenerator = new SensorDataGenerator();
            _producerPolicyOptionsMock = new Mock<IOptions<ProducerPolicy>>();
            _loggerMock = new Mock<ILogger<ControlService>>();
            _eventRepositoryMock = new Mock<IEventRepository>();
            _mapperMock = new Mock<IMapper>();

            var producerPolicy = new ProducerPolicy
            {
                RetryLookbackSeconds = 3,
                ResendDelayInSeconds = 100,
            };
            _producerPolicyOptionsMock.Setup(x => x.Value).Returns(producerPolicy);

            _controlService = new ControlService(
                _sendServiceMock.Object,
                _resendServiceMock.Object,
                _sensorServiceMock.Object,
                _messageProducerMock.Object,
                _sensorDataGenerator,
                _producerPolicyOptionsMock.Object,
                _loggerMock.Object,
                _eventRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        private double GenerateSensorData(int value)
        {
            return value;
        }

        [Fact]
        public async Task AppStartAsync_SetsIsRunningToTrue()
        {
            // Arrange
            var sensors = new List<Sensor>
                {
                    new Sensor
                    {
                        SensorId = "1",
                        Interval = 2,
                        MeasurementType = MeasurementType.Temperature,
                        GenerateData = () => GenerateSensorData(5)
                    }
                };
            _sensorServiceMock.Setup(x => x.GetSensors()).Returns(sensors);

            // Act
            await _controlService.AppStartAsync();

            // Assert
            Assert.True(_controlService.IsRunning);
        }

        [Fact]
        public async Task AppStopAsync_SetsIsRunningToFalse()
        {
            // Arrange
            await _controlService.AppStartAsync();

            // Act
            await _controlService.AppStopAsync();

            // Assert
            Assert.False(_controlService.IsRunning);
        }
    }
}


using AutoMapper;
using Ifolor.ProducerService.Core.Models;
using IfolorProducerService.Application.Mapping;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ifolor.ProducerService.Tests
{
    public class ResendServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepository;
        private readonly Mock<ISendService> _mockSendService;
        private readonly Mock<ILogger<ResendService>> _mockLogger;
        private readonly ResendService _resendService;

        public ResendServiceTests()
        {
            _mockEventRepository = new Mock<IEventRepository>();
            _mockSendService = new Mock<ISendService>();
            _mockLogger = new Mock<ILogger<ResendService>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();

            _resendService = new ResendService(
                _mockEventRepository.Object,
                 mapper,
                _mockSendService.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public void HandleUnsendMessages_SkipsWhenResendDelayIsZero()
        {
            // Arrange
            int resendDelayInSeconds = 0;
            var token = CancellationToken.None;

            // Act
            _resendService.HandleUnsendMessages(resendDelayInSeconds, token);

            // Assert
            _mockEventRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task HandleUnsendMessages_ProcessesUnsentMessages()
        {
            // Arrange
            int resendDelayInSeconds = 5;
            var token = CancellationToken.None;

            var unsendEvents = new List<SensorEventEntity>
            {
                new SensorEventEntity { EventId = Guid.NewGuid(), SensorId = "Sensor-001", EventStatus = EventStatus.NotSend }
            };

            var eventDataList = new List<SensorData>
            {
                new SensorData { EventId = unsendEvents[0].EventId, SensorId = "Sensor-001" }
            };

            _mockEventRepository
                .Setup(r => r.GetUnsendMessages())
                .ReturnsAsync(unsendEvents);

            _mockEventRepository
                .Setup(r => r.HasNotSentMessages())
                .ReturnsAsync(true);

            // Act
            _resendService.HandleUnsendMessages(resendDelayInSeconds, token);

            // Wait for the periodic task to run
            await Task.Delay(1000);

            // Assert
            _mockEventRepository.Verify(r => r.GetUnsendMessages(), Times.AtLeastOnce);
            _mockSendService.Verify(s => s.SendToMessageBroker(It.IsAny<SensorData>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RunPeriodically_StopsOnCancellation()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _mockEventRepository
                .Setup(r => r.HasNotSentMessages())
                .ReturnsAsync(true);

            // Act
            _resendService.HandleUnsendMessages(1, token);
            cts.Cancel();

            // Wait for the task to stop
            await Task.Delay(1000);

            // Assert
            _mockEventRepository.Verify(r => r.HasNotSentMessages(), Times.AtLeastOnce);
        }

    }
}

using Ifolor.ProducerService.Core.Models;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Ifolor.ProducerService.Tests
{
    public class EventRepositoryTests
    {
        private readonly Mock<IDbContextFactory<ProducerDbContext>> _mockContextFactory;
        private readonly Mock<ILogger<EventRepository>> _mockLogger;
        private readonly Mock<IDateTimeProvider> _mockDateTimeProvider;
        private readonly EventRepository _eventRepository;
        private readonly ProducerPolicy _producerPolicy;

        public EventRepositoryTests()
        {
            _mockContextFactory = new Mock<IDbContextFactory<ProducerDbContext>>();
            _mockLogger = new Mock<ILogger<EventRepository>>();
            _mockDateTimeProvider = new Mock<IDateTimeProvider>();

            _producerPolicy = new ProducerPolicy { RetryLookbackSeconds = 60 };
            var mockOptions = new Mock<IOptions<ProducerPolicy>>();
            mockOptions.Setup(o => o.Value).Returns(_producerPolicy);

            _eventRepository = new EventRepository(
                _mockContextFactory.Object,
                mockOptions.Object,
                _mockLogger.Object,
                _mockDateTimeProvider.Object
            );
        }

        private ProducerDbContext CreateInMemoryDbContext(string testDb)
        {
            var options = new DbContextOptionsBuilder<ProducerDbContext>()
                .UseInMemoryDatabase(databaseName: testDb)
                .Options;

            return new ProducerDbContext(options);
        }

        [Fact]
        public async Task SaveSensorDataAsync_SavesDataSuccessfully()
        {
            // Arrange
            var sensorData = new SensorData
            {
                EventId = Guid.NewGuid(),
                SensorId = "Sensor-001",
                MeasurementType = MeasurementType.Temperature,
                MeasurementValue = 25.5,
                EventStatus = EventStatus.Send
            };

            var dbContext = CreateInMemoryDbContext("testdb");
            _mockContextFactory
                .Setup(f => f.CreateDbContext())
                .Returns(dbContext);

            // Act
            await _eventRepository.SaveSensorDataAsync(sensorData);

            // Assert
            using (dbContext = CreateInMemoryDbContext("testdb"))
            {
                var savedEntity = await dbContext.SensorData.FirstOrDefaultAsync();
                Assert.NotNull(savedEntity);
                Assert.Equal(sensorData.EventId, savedEntity.EventId);
                Assert.Equal(sensorData.SensorId, savedEntity.SensorId);
            }
        }

        [Fact]
        public async Task HasNotSentMessages_ReturnsTrueWhenUnsentMessagesExist()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

            var dbContext = CreateInMemoryDbContext("testdb");
            dbContext.SensorData.Add(new SensorEventEntity
            {
                EventId = Guid.NewGuid(),
                SensorId = "Sensor-001",
                Timestamp = now,
                EventStatus = EventStatus.NotSend
            });
            await dbContext.SaveChangesAsync();

            _mockContextFactory
                .Setup(f => f.CreateDbContext())
                .Returns(dbContext);

            _mockDateTimeProvider
                .Setup(p => p.UtcNow)
                .Returns(now);

            // Act
            var result = await _eventRepository.HasNotSentMessages();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetUnsendMessages_ReturnsUnsentMessages()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

            var dbContext = CreateInMemoryDbContext("testdb3");
            dbContext.SensorData.Add(new SensorEventEntity
            {
                EventId = Guid.NewGuid(),
                SensorId = "Sensor-001",
                Timestamp = now,
                EventStatus = EventStatus.NotSend
            });
            await dbContext.SaveChangesAsync();

            _mockContextFactory
                .Setup(f => f.CreateDbContext())
                .Returns(dbContext);

            _mockDateTimeProvider
                .Setup(p => p.UtcNow)
                .Returns(now);

            // Act
            var result = await _eventRepository.GetUnsendMessages();

            // Assert
            Assert.Single(result);
            Assert.Equal("Sensor-001", result[0].SensorId);
        }

        [Fact]
        public async Task SaveSensorDataAsync_HandlesDbUpdateException()
        {
            // Arrange
            var sensorData = new SensorData
            {
                EventId = Guid.NewGuid(),
                SensorId = "Sensor-001",
                MeasurementType = MeasurementType.Temperature,
                MeasurementValue = 25.5,
                EventStatus = EventStatus.Send
            };

            var dbContext = CreateInMemoryDbContext("testdb");
            _mockContextFactory
                .Setup(f => f.CreateDbContext())
                .Returns(dbContext);

            // Simulate a NullReferenceException
            dbContext.SensorData = null; 

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _eventRepository.SaveSensorDataAsync(sensorData));
        }
    }
}

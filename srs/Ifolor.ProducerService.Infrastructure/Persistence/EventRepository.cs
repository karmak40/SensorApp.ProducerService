using Ifolor.ProducerService.Infrastructure.Messaging;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbContextFactory<ProducerDbContext> _contextFactory;
        private readonly ProducerPolicy _producerPolicy;
        private readonly ILogger<EventRepository> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;

        public EventRepository(
            IDbContextFactory<ProducerDbContext> contextFactory, 
            IOptions<ProducerPolicy> producerPolicy,
            ILogger<EventRepository> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _contextFactory = contextFactory;
            _producerPolicy = producerPolicy.Value;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task SaveSensorDataAsync(SensorData sensorData)
        {
            var entity = new SensorEventEntity
            {
                EventId = sensorData.EventId,
                SensorId = sensorData.SensorId,
                Timestamp = DateTime.UtcNow,
                MeasurementType = sensorData.MeasurementType,
                MeasurementValue = sensorData.MeasurementValue,
                EventStatus = sensorData.EventStatus,
            };

            try
            {
                using var context = _contextFactory.CreateDbContext();
                context.SensorData.Add(entity);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving sensor data for sensor {SensorId}", sensorData.SensorId);
                throw;
            }
            catch (NullReferenceException ex) 
            {
                _logger.LogError(ex, "Error saving sensor data for sensor {SensorId} argument is null", sensorData.SensorId);
                throw;
            }
        }

        public async Task<bool> HasNotSentMessages()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var now = _dateTimeProvider.UtcNow;
                var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

                return await context.SensorData
                    .AsNoTracking()
                    .AnyAsync(data => data.EventStatus == EventStatus.NotSend &&
                                      data.Timestamp >= pastTime &&
                                      data.Timestamp <= now);
            }
        }

        public async Task<List<SensorEventEntity>> GetUnsendMessages()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var now = _dateTimeProvider.UtcNow;
                var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

                return await context.SensorData
                    .AsNoTracking()
                    .Where(data => data.EventStatus == EventStatus.NotSend &&
                                   data.Timestamp >= pastTime &&
                                   data.Timestamp <= now)
                    .ToListAsync();
            }
        }
    }
}

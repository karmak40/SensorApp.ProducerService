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

        public async Task UpdateEventStatusAsync(Guid eventId, EventStatus newStatus)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();

                // Find the event by EventId
                var eventToUpdate = await context.SensorData
                    .FirstOrDefaultAsync(e => e.EventId == eventId);

                if (eventToUpdate != null)
                {
                    // Update the EventStatus
                    eventToUpdate.EventStatus = newStatus;

                    // Save changes to the database
                    await context.SaveChangesAsync();

                    _logger.LogInformation("Event {EventId} status updated to {NewStatus}.", eventId, newStatus);
                }
                else
                {
                    _logger.LogWarning("Event with ID {EventId} not found.", eventId);
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for event {EventId}", eventId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating status for event {EventId}", eventId);
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

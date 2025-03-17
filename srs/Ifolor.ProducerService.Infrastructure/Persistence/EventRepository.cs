using Ifolor.ProducerService.Infrastructure.Messaging;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbContextFactory<ProducerDbContext> _contextFactory;
        private readonly ProducerPolicy _producerPolicy;

        public EventRepository(IDbContextFactory<ProducerDbContext> contextFactory, IOptions<ProducerPolicy> producerPolicy)
        {
            _contextFactory = contextFactory;
            _producerPolicy = producerPolicy.Value;
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

            using (var context = _contextFactory.CreateDbContext())
            {
                context.SensorData.Add(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasNotSentMessages()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var now = DateTime.UtcNow;
                var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

                return await context.SensorData
                .AnyAsync(data => data.EventStatus == EventStatus.NotSend &&
                             data.Timestamp >= pastTime &&
                             data.Timestamp <= now);

            }
        }

        public async Task<List<SensorEventEntity>> GetUnsendMessages()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var now = DateTime.UtcNow;
                var pastTime = now.AddSeconds(-_producerPolicy.RetryLookbackSeconds);

                var sensorDataEvents = await context.SensorData
                .Where(data => data.EventStatus == EventStatus.NotSend &&
                             data.Timestamp >= pastTime &&
                             data.Timestamp <= now).ToListAsync();

                return sensorDataEvents;

            }
        }
    }
}

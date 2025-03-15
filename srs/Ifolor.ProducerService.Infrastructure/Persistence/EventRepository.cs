using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Events;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbContextFactory<EventDbContext> _contextFactory;

        public EventRepository(IDbContextFactory<EventDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SaveEventAsync(IEvent @event)
        {
            var entity = new EventEntity
            {
                EventId = @event.EventId,
                Timestamp = @event.Timestamp,
                EventType = @event.EventType,
                Data = JsonSerializer.Serialize(@event)
            };

            using (var context = _contextFactory.CreateDbContext())
            {
                context.Events.Add(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveSensorDataAsync(SensorData sensorData)
        {
            var entity = new SensorEventEntity
            {
                EventId = sensorData.EventId,
                SensorId = sensorData.SensorId,
                Timestamp = DateTime.UtcNow,
                MeasurementType = sensorData.MeasurementType,
                MeasurementValue = sensorData.MeasurementValue
            };

            using (var context = _contextFactory.CreateDbContext())
            {
                context.SensorData.Add(entity);
                await context.SaveChangesAsync();
            }

        }
    }
}

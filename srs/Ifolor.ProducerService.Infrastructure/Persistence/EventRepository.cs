using IfolorProducerService.Core.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventRepository: IEventRepository
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
    }
}

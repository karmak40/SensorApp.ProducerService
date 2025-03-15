using Microsoft.EntityFrameworkCore;
using System;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventDbContext: DbContext
    {
        public DbSet<EventEntity> Events { get; set; }

        public EventDbContext(DbContextOptions<EventDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>().HasKey(e => e.EventId);
        }
    }

    public class EventEntity
    {
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; } // JSON-строка события
    }
}

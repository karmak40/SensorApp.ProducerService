using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class EventDbContext: DbContext
    {
        public DbSet<EventEntity> Events { get; set; }

        public DbSet<SensorEventEntity> SensorData { get; set; }

        public EventDbContext(DbContextOptions<EventDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventEntity>().HasKey(e => e.EventId);
            modelBuilder.Entity<SensorEventEntity>().HasKey(e => e.EventId);
        }
    }

    public class EventEntity
    {
        public Guid EventId { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }
        public string Data { get; set; } // JSON-строка события
    }

    [Table("SensorData")]

    public class SensorEventEntity
    {
        public Guid EventId { get; set; }
        public string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public string MeasurementType { get; set; }
        public double MeasurementValue { get; set; }
    }
}

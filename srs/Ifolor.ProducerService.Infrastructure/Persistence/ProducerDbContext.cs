using IfolorProducerService.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class ProducerDbContext: DbContext
    {
        public DbSet<SensorEventEntity> SensorData { get; set; }

        public ProducerDbContext(DbContextOptions<ProducerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorEventEntity>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.EventStatus)
                      .HasConversion<string>();

                entity.Property(e => e.MeasurementType)
                        .HasConversion<string>();
            });

        }
    }


    [Table("SensorData")]

    public class SensorEventEntity
    {
        public int Id { get; set; }
        public Guid EventId { get; set; }
        public EventStatus EventStatus { get; set; }
        public required string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public double MeasurementValue { get; set; }
    }
}

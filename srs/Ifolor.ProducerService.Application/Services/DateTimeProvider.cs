namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}

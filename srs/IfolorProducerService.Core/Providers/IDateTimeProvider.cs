namespace Ifolor.ProducerService.Infrastructure.Persistence
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}

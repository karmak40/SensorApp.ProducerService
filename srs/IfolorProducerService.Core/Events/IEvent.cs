
namespace IfolorProducerService.Core.Events
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
        string EventType { get; }
        public int EventNumber { get; }
        public string Data { get; }
    }
}

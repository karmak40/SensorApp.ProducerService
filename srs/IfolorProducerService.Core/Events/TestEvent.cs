using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Core.Events
{
    public class TestEvent: IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string EventType => "TestEvent";
        public int EventNumber { get; }
        public string Data { get; }

        public TestEvent(int eventNumber, string data)
        {
            EventNumber = eventNumber;
            Data = data;
        }
    }
}

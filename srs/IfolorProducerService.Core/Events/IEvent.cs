using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Core.Events
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
        string EventType { get; }
    }
}

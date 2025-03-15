using IfolorProducerService.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Core.Generators
{
    public class TestEventGenerator : IEventGenerator
    {
        private int _counter = 0;

        public Task<IEvent> GenerateAsync()
        {
            var eventNumber = Interlocked.Increment(ref _counter);
            return Task.FromResult<IEvent>(new TestEvent(eventNumber, $"Test event {eventNumber}"));
        }
    }
}

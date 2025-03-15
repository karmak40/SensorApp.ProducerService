using IfolorProducerService.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Core.Generators
{
    public interface IEventGenerator
    {
        Task<IEvent> GenerateAsync();
    }
}

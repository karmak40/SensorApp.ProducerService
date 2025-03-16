using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifolor.ProducerService.Infrastructure.Messaging
{
    public class ProducerPolicy
    {
        public int RetryLookbackSeconds { get; set; }

        public int ResendDelayInSeconds { get; set; }
    }
}

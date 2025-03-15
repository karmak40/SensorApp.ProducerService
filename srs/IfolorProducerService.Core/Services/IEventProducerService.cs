using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Application.Services
{
    public interface IEventProducerService
    {
        Task StartAsync();
        Task StopAsync();
        bool IsRunning { get; }
    }
}

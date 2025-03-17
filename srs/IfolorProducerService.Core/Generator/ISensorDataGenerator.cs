using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Core.Generator
{
    public interface ISensorDataGenerator
    {
        SensorData GenerateData(Sensor sensor);
    }
}

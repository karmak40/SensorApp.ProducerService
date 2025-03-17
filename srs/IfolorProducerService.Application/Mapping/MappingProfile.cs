using AutoMapper;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfolorProducerService.Application.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // Create a mapping from SourceObject to DestinationObject
            CreateMap<SensorEventEntity, SensorData>()   
                ;
        }
    }
}

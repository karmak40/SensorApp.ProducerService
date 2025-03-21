using AutoMapper;
using Ifolor.ProducerService.Core.Models;
using IfolorProducerService.Application.Services;

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

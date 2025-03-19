using AutoMapper;
using Ifolor.ProducerService.Infrastructure.Persistence;
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

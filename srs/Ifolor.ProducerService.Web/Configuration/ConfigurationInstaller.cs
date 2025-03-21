using Ifolor.ProducerService.Application.Installer;
using Ifolor.ProducerService.Infrastructure.Installer;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Mapping;

namespace Ifolor.ProducerService.Web.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMQConfig>(configuration.GetSection("RabbitMQ"));
            services.Configure<ProducerPolicy>(configuration.GetSection("Producerpolicy"));
            services.Configure<SQLiteConfig>(configuration.GetSection("SQLite"));

            return services;
        }

        public static IServiceCollection AddSystemsServices(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            return services;
        }

        public static IServiceCollection AddDunkelverarbeitungs (this IServiceCollection services)
        {
            services.AddInfrastructureServices();
            services.AddApplicationServices();

            return services;
        }
    }
}

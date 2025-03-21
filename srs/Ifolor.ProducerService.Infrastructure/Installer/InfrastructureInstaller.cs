using Ifolor.ProducerService.Core.Models;
using Ifolor.ProducerService.Core.Services;
using Ifolor.ProducerService.Infrastructure.Messaging;
using Ifolor.ProducerService.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Ifolor.ProducerService.Infrastructure.Installer
{
    public static class InfrastructureInstaller
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IMessageProducer, RabbitMQProducer>();

            return services;
        }
    }
}

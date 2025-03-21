using Ifolor.ProducerService.Infrastructure.Persistence;
using IfolorProducerService.Application.Generator;
using IfolorProducerService.Application.Services;
using IfolorProducerService.Core.Generator;
using IfolorProducerService.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ifolor.ProducerService.Application.Installer
{
    public static class ApplicationInstaller
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddTransient<ISendService, SendService>();
            services.AddTransient<IResendService, ResendService>();
            services.AddTransient<ISensorDataGenerator, SensorDataGenerator>();
            services.AddTransient<ISensorService, SensorService>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient<IControlService, ControlService>();

            return services;
        }
    }
}

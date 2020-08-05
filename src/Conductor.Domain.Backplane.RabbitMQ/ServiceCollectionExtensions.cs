using System;
using Conductor.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor.Domain.Backplane.RabbitMQ
{
    public static class ServiceCollectionExtensions
    {
        public static void UseRabbitMQBackplane(this IServiceCollection services, string url)
        {
            services.AddSingleton<IClusterBackplane, RabbitMQClusterBackplane>();
        }
    }
}
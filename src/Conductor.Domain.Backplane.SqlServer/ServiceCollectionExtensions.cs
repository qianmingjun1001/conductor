using System;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;

namespace Conductor.Domain.Backplane.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static void UseSqlServerBackplane(this IServiceCollection services, [NotNull] string connectionString,
            [CanBeNull] Action<IServiceProvider, string> registryDynamicRouteCallback)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            services.AddSingleton<IClusterBackplane>(p =>
                new SqlServerClusterBackplane(
                    connectionString,
                    p,
                    registryDynamicRouteCallback,
                    p.GetRequiredService<IFlowDefinitionService>(),
                    p.GetRequiredService<IWorkflowLoader>(),
                    p.GetRequiredService<IWorkflowRegistry>(),
                    p.GetRequiredService<ILoggerFactory>())
            );
        }
    }
}
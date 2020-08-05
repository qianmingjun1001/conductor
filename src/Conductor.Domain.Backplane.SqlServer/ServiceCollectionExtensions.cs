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
        public static void UseSqlServerBackplane(this IServiceCollection services, [NotNull] string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            services.AddSingleton<IClusterBackplane>(p =>
                new SqlServerClusterBackplane(
                    connectionString,
                    p.GetRequiredService<IDefinitionRepository>(),
                    p.GetRequiredService<IWorkflowLoader>(),
                    p.GetRequiredService<IWorkflowRegistry>(),
                    p.GetRequiredService<ILoggerFactory>())
            );
        }
    }
}
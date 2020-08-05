using System;
using Conductor.Domain.Interfaces;
using Conductor.Storage.SqlServer.Services;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor.Storage.SqlServer
{
    public static class ServiceCollectionExtensions
    {
        public static void UseSqlServer(this IServiceCollection services, [NotNull] string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            services.AddSingleton<WorkflowDbContextFactory>(new WorkflowDbContextFactory(connectionString));
            services.AddSingleton<IFlowDefinitionRepository, FlowDefinitionRepository>();
            services.AddSingleton<IDefinitionRepository, FlowDefinitionRepository>();
        }
    }
}
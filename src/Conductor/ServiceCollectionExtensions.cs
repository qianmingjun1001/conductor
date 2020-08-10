using System;
using System.Collections.Generic;
using System.Text;
using Conductor.DynamicRoute;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor
{
    public static class ServiceCollectionExtensions
    {
        public static void AddEntryPointRoute([NotNull] this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<EntryPointRouteRegistry>();
        }
    }
}
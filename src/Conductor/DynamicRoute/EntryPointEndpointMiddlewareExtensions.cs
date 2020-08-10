using System;
using System.Collections.Generic;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor.DynamicRoute
{
    public static class EntryPointEndpointMiddlewareExtensions
    {
        public static void MapEntryPoint([NotNull] this IEndpointRouteBuilder endpointRouteBuilder)
        {
            if (endpointRouteBuilder == null) throw new ArgumentNullException(nameof(endpointRouteBuilder));

            var app = endpointRouteBuilder.CreateApplicationBuilder();
            var requestDelegate = app
                .UseMiddleware<EntryPointEndpointMiddleware>()
                .Build();

            var dynamicRouteRegistry = app.ApplicationServices.GetRequiredService<EntryPointRouteRegistry>();
            dynamicRouteRegistry.Initialize(requestDelegate);

            var flowDefinitionService = app.ApplicationServices.GetRequiredService<IFlowDefinitionService>();
            foreach (var path in flowDefinitionService.GetAllEntryPointPath().Result)
            {
                dynamicRouteRegistry.RegisterRoute(path);
            }

            endpointRouteBuilder.DataSources.Add(dynamicRouteRegistry.DataSource);
        }
    }
}
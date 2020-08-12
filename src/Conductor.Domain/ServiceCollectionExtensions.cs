using System;
using System.Reflection;
using Conductor.Domain.Interfaces;
using Conductor.Domain.ReplacedServices;
using Conductor.Domain.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDomainServices(this IServiceCollection services)
        {
            services.AddSingleton<ICache, MemoryCache>();
            services.AddSingleton<IDefinitionService, DefinitionService>();
            services.AddSingleton<IWorkflowLoader, WorkflowLoader>();
            // services.AddSingleton<ICustomStepService, CustomStepService>()
            services.AddSingleton<ICustomStepService, ReplacedCustomStepService>();

            // services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
            services.AddSingleton<IExpressionEvaluator, ReplacedExpressionEvaluator>();
            services.AddTransient<CustomStep>();

            services.AddSingleton<IFlowDefinitionService, FlowDefinitionService>();
            services.AddMediatR(Assembly.GetEntryAssembly(), typeof(ServiceCollectionExtensions).Assembly);
        }
    }
}
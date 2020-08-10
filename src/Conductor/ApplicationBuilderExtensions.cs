using Conductor.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WorkflowCore.Interface;

namespace Conductor
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseWorkflow(this IApplicationBuilder app)
        {
            var host = app.ApplicationServices.GetRequiredService<IWorkflowHost>();
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            var defService = app.ApplicationServices.GetRequiredService<IFlowDefinitionService>();
            var backplane = app.ApplicationServices.GetRequiredService<IClusterBackplane>();

            defService.LoadDefinitionsFromStorage().Wait();
            backplane.Start().Wait();
            host.Start();
            applicationLifetime.ApplicationStopped.Register(() =>
            {
                host.Stop();
                backplane.Stop().Wait();
            });
        }
    }
}
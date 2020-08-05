using Conductor.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Conductor.Domain.Scripting.ExpressionTree
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureExpressionTreeScripting(this IServiceCollection services)
        {
            services.AddSingleton<IScriptEngineHost, ScriptEngineHost>();
        }
    }
}

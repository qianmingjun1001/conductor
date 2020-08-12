using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Conductor.DynamicRoute
{
    public class EntryPointRouteRegistry
    {
        private readonly List<string> _patterns = new List<string>();
        private RequestDelegate _requestDelegate;

        public DynamicEndpointDataSource DataSource { get; } = new DynamicEndpointDataSource();

        public void Initialize([NotNull] RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate ?? throw new ArgumentNullException(nameof(requestDelegate));
        }

        public void RegisterRoute([NotNull] string pattern)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            EnsuredInitialized();

            var temp = pattern.ToLower();
            if (_patterns.Exists(p => p == temp))
            {
                return;
            }

            var routeBuilder = new RouteEndpointBuilder(_requestDelegate, RoutePatternFactory.Parse(pattern), 0);
            routeBuilder.DisplayName = $"[EntryPoint: {pattern}]";
            DataSource.AddEndpoint(routeBuilder.Build());

            _patterns.Add(temp);
        }

        private void EnsuredInitialized()
        {
            if (_requestDelegate == null)
            {
                throw new InvalidOperationException("调用方法之前请先调用 Initialize 方法进行初始化");
            }
        }
    }
}
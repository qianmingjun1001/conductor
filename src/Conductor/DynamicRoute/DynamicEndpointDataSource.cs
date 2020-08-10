using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace Conductor.DynamicRoute
{
    public class DynamicEndpointDataSource : EndpointDataSource
    {
        private readonly List<Endpoint> _endpoints;
        private CancellationTokenSource _cts;
        private CancellationChangeToken _changeToken;
        private readonly object _lock;

        public DynamicEndpointDataSource(params Endpoint[] endpoints)
        {
            _endpoints = new List<Endpoint>();
            _endpoints.AddRange(endpoints);
            _lock = new object();

            CreateChangeToken();
        }

        public override IChangeToken GetChangeToken() => _changeToken;

        public override IReadOnlyList<Endpoint> Endpoints => _endpoints;

        // Trigger change
        public void AddEndpoint([NotNull] Endpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            lock (_lock)
            {
                _endpoints.Add(endpoint);

                // Capture the old tokens so that we can raise the callbacks on them. This is important so that
                // consumers do not register callbacks on an inflight event causing a stackoverflow.
                var oldTokenSource = _cts;
                var oldToken = _changeToken;

                CreateChangeToken();

                // Raise consumer callbacks. Any new callback registration would happen on the new token
                // created in earlier step.
                oldTokenSource.Cancel();
                oldTokenSource.Dispose();
            }
        }

        private void CreateChangeToken()
        {
            _cts = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cts.Token);
        }
    }
}
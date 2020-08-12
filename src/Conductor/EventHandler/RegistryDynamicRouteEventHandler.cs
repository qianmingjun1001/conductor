using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.EventData;
using Conductor.DynamicRoute;
using MediatR;

namespace Conductor.EventHandler
{
    public class RegistryDynamicRouteEventHandler : INotificationHandler<CreateFlowDefinitionEventData>, INotificationHandler<UpdateFlowDefinitionEventData>
    {
        private readonly EntryPointRouteRegistry _entryPointRouteRegistry;

        public RegistryDynamicRouteEventHandler(EntryPointRouteRegistry entryPointRouteRegistry)
        {
            _entryPointRouteRegistry = entryPointRouteRegistry;
        }

        public Task Handle(CreateFlowDefinitionEventData notification, CancellationToken cancellationToken)
        {
            RegistryDynamicRoute(notification.EntryPointPath);

            return Task.CompletedTask;
        }

        public Task Handle(UpdateFlowDefinitionEventData notification, CancellationToken cancellationToken)
        {
            RegistryDynamicRoute(notification.NewEntryPointPath);

            return Task.CompletedTask;
        }

        public void RegistryDynamicRoute(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                _entryPointRouteRegistry.RegisterRoute(path);
            }
        }
    }
}
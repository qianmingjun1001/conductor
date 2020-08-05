using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.EventData;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using MediatR;
using WorkflowCore.Interface;

namespace Conductor.Domain.EventHandler
{
    /// <summary>
    /// 修改流程事件处理
    /// </summary>
    public class UpdateFlowDefinitionEventHandler : INotificationHandler<UpdateFlowDefinitionEventData>
    {
        [NotNull]
        private readonly IWorkflowRegistry _workflowRegistry;

        [NotNull]
        private readonly IWorkflowLoader _workflowLoader;

        [NotNull]
        private readonly IClusterBackplane _backplane;

        public UpdateFlowDefinitionEventHandler([NotNull] IWorkflowRegistry workflowRegistry, [NotNull] IWorkflowLoader workflowLoader, [NotNull] IClusterBackplane backplane)
        {
            _workflowRegistry = workflowRegistry;
            _workflowLoader = workflowLoader;
            _backplane = backplane;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle(UpdateFlowDefinitionEventData notification, CancellationToken cancellationToken)
        {
            var definition = notification.Definition;
            var id = definition.Id;
            var version = definition.Version;

            if (_workflowRegistry.IsRegistered(id, version))
            {
                _workflowRegistry.DeregisterWorkflow(id, version);
            }

            _workflowLoader.LoadDefinition(definition);
            _backplane.LoadNewDefinition(id, version);

            return Task.CompletedTask;
        }
    }
}
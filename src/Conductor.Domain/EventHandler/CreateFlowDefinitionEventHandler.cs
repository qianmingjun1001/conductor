using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.EventData;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using MediatR;

namespace Conductor.Domain.EventHandler
{
    /// <summary>
    /// 创建流程事件处理
    /// </summary>
    public class CreateFlowDefinitionEventHandler : INotificationHandler<CreateFlowDefinitionEventData>
    {
        [NotNull]
        private readonly IWorkflowLoader _workflowLoader;

        [NotNull]
        private readonly IClusterBackplane _backplane;

        public CreateFlowDefinitionEventHandler([NotNull] IWorkflowLoader workflowLoader, [NotNull] IClusterBackplane backplane)
        {
            _workflowLoader = workflowLoader;
            _backplane = backplane;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle(CreateFlowDefinitionEventData notification, CancellationToken cancellationToken)
        {
            var definition = notification.Definition;
            _workflowLoader.LoadDefinition(definition);
            _backplane.LoadNewDefinition(definition.Id, definition.Version);
            return Task.CompletedTask;
        }
    }
}
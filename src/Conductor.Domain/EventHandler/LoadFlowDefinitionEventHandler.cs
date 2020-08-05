using System.Threading;
using System.Threading.Tasks;
using Conductor.Domain.EventData;
using Conductor.Domain.Interfaces;
using JetBrains.Annotations;
using MediatR;

namespace Conductor.Domain.EventHandler
{
    /// <summary>
    /// 加载流程事件处理
    /// </summary>
    public class LoadFlowDefinitionEventHandler : INotificationHandler<LoadFlowDefinitionEventData>
    {
        [NotNull]
        private readonly IWorkflowLoader _workflowLoader;

        public LoadFlowDefinitionEventHandler([NotNull] IWorkflowLoader workflowLoader, [NotNull] IClusterBackplane backplane)
        {
            _workflowLoader = workflowLoader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle(LoadFlowDefinitionEventData notification, CancellationToken cancellationToken)
        {
            var definition = notification.Definition;
            _workflowLoader.LoadDefinition(definition);
            return Task.CompletedTask;
        }
    }
}
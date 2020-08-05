using Conductor.Domain.Models;
using JetBrains.Annotations;
using MediatR;

namespace Conductor.Domain.EventData
{
    /// <summary>
    /// 创建流程事件数据
    /// </summary>
    public class LoadFlowDefinitionEventData : INotification
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        public LoadFlowDefinitionEventData([NotNull] Definition definition)
        {
            Definition = definition;
        }

        /// <summary>
        /// 流程定义
        /// </summary>
        public Definition Definition { get; }
    }
}
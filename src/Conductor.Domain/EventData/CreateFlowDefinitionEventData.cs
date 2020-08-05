using Conductor.Domain.Models;
using JetBrains.Annotations;
using MediatR;

namespace Conductor.Domain.EventData
{
    /// <summary>
    /// 创建流程事件数据
    /// </summary>
    public class CreateFlowDefinitionEventData : INotification
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        public CreateFlowDefinitionEventData([NotNull] Definition definition)
        {
            Definition = definition;
        }

        /// <summary>
        /// 流程定义
        /// </summary>
        public Definition Definition { get; }
    }
}
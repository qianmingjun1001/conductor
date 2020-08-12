using Conductor.Domain.Models;
using JetBrains.Annotations;
using MediatR;

namespace Conductor.Domain.EventData
{
    /// <summary>
    /// 修改流程事件数据
    /// </summary>
    public class UpdateFlowDefinitionEventData : INotification
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="oldEntryPointPath"></param>
        /// <param name="newEntryPointPath"></param>
        public UpdateFlowDefinitionEventData([NotNull] Definition definition, string oldEntryPointPath, string newEntryPointPath)
        {
            Definition = definition;
            OldEntryPointPath = oldEntryPointPath;
            NewEntryPointPath = newEntryPointPath;
        }

        /// <summary>
        /// 流程定义
        /// </summary>
        public Definition Definition { get; }

        /// <summary>
        /// 旧入口点
        /// </summary>
        public string OldEntryPointPath { get; }

        /// <summary>
        /// 新入口点
        /// </summary>
        public string NewEntryPointPath { get; }
    }
}
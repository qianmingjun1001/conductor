using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Conductor.Domain.Entities;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;

namespace Conductor.Dtos
{
    /// <summary>
    /// 工作流定义输入
    /// </summary>
    public class FlowDefinitionInput
    {
        /// <summary>
        /// 流程 ID
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FlowName { get; set; }

        /// <summary>
        /// 工作流定义
        /// </summary>
        [Required]
        public Definition Definition { get; set; }

        /// <summary>
        /// 入口点
        /// </summary>
        public EntryPoint EntryPoint { get; set; }

        public FlowDefinition ToFlowDefinition()
        {
            return new FlowDefinition
            {
                FlowId = FlowId,
                FlowName = FlowName,
                Definition = JsonUtils.Serialize(Definition),
                DefinitionId = Definition?.Id,
                DefinitionVersion = Definition?.Version ?? 0,
                Description = Definition?.Description,
                EntryPoint = JsonUtils.Serialize(EntryPoint),
                EntryPointPath = EntryPoint?.Path
            };
        }
    }
}
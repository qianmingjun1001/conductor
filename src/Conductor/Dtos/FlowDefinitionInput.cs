using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Conductor.Domain.Entities;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public JObject Definition { get; set; }

        /// <summary>
        /// 入口点
        /// </summary>
        [Required]
        [JsonProperty(PropertyName = "consumerStep")]
        public EntryPoint EntryPoint { get; set; }

        public FlowDefinition ToFlowDefinition()
        {
            return new FlowDefinition
            {
                FlowId = FlowId,
                FlowName = FlowName,
                Definition = JsonUtils.Serialize(Definition),
                DefinitionId = Definition["id"].ToObject<string>(),
                DefinitionVersion = Definition["version"].ToObject<int>(),
                Description = Definition["description"].ToObject<string>(),
                EntryPoint = JsonUtils.Serialize(EntryPoint),
                EntryPointPath = EntryPoint?.Inputs?["path"]
            };
        }
    }
}
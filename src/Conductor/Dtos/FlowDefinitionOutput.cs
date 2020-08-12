using System;
using System.Collections.Generic;
using System.Text;
using Conductor.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conductor.Dtos
{
    /// <summary>
    /// 工作流定义输出
    /// </summary>
    public class FlowDefinitionOutput
    {
        /// <summary>
        /// 流程 id
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 工作流定义
        /// </summary>
        public JObject Definition { get; set; }

        /// <summary>
        /// 入口点
        /// </summary>
        [JsonProperty(PropertyName = "consumerStep")]
        public EntryPoint EntryPoint { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
    }
}
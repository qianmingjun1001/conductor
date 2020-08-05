using System;
using System.Collections.Generic;
using System.Text;
using Conductor.Domain.Models;

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
        public int FlowId { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 工作流定义
        /// </summary>
        public Definition Definition { get; set; }

        /// <summary>
        /// 入口点
        /// </summary>
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
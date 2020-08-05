using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 工作流上下文
    /// </summary>
    [Keywords("data")]
    public class WorkflowContext
    {
        /// <summary>
        /// 有效载荷
        /// </summary>
        [Keywords("payload")]
        public AnyObject Payload { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        [Keywords("attributes")]
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        [Keywords("vars")]
        public Dictionary<string, string> Variables { get; set; }

        /// <summary>
        /// 存储输出结果
        /// </summary>
        public Dictionary<string, object> Outputs { get; } = new Dictionary<string, object>();

        public DateTime Now => DateTime.Now;
    }
}
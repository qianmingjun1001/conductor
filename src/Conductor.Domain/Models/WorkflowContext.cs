using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Text;
using Conductor.Domain.Utils;
using Newtonsoft.Json;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 工作流上下文
    /// </summary>
    public class WorkflowContext
    {
        private DynamicClass _internalPayloadClass;

        /// <summary>
        /// 任意类型
        /// </summary>
        public ExpandoObject Payload { get; set; } = new ExpandoObject();

        /// <summary>
        /// Payload 真实类型
        /// </summary>
        [JsonIgnore]
        public DynamicClass PayloadClass => _internalPayloadClass = _internalPayloadClass ?? Payload.ToDynamicClass();

        /// <summary>
        /// 属性
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 变量
        /// </summary>
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 存储输出结果
        /// </summary>
        public Dictionary<string, object> Outputs { get; } = new Dictionary<string, object>();

        public DateTime Now => DateTime.Now;

        public Type GetPayloadType()
        {
            return Payload == null ? typeof(DynamicClass) : PayloadClass.GetType();
        }
    }
}
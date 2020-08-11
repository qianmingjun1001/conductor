using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Runtime.Serialization;
using System.Text;
using Conductor.Domain.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 工作流上下文
    /// </summary>
    public class WorkflowContext
    {
        /// <summary>
        /// Payload
        /// </summary>
        public ExpandoObject Payload { get; set; }

        /// <summary>
        /// Payload 真实类型
        /// </summary>
        // [JsonIgnore]
        public DynamicClass PayloadClass { get; set; }

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

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (PayloadClass == null)
            {
                PayloadClass = Payload?.ToDynamicClass();
            }
        }

        [OnError]
        public void OnError(StreamingContext context, ErrorContext errorContext)
        {
            //如果是无法加载匿名类型
            if ("PayloadClass".Equals(errorContext.Member)
                && errorContext.Error is JsonSerializationException exception
                && exception.Path == "PayloadClass.$type")
            {
                PayloadClass = Payload?.ToDynamicClass();
                errorContext.Handled = true;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;
using Newtonsoft.Json.Linq;

namespace Conductor.Dtos
{
    /// <summary>
    /// 工作流上下文 Dto
    /// </summary>
    public class WorkflowContextDto
    {
        /// <summary>
        /// Payload
        /// </summary>
        public JObject Payload { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        public Dictionary<string, string> Variables { get; set; }

        public WorkflowContext ToWorkflowContext()
        {
            var context = new WorkflowContext();
            if (Payload != null)
            {
                context.Payload = Payload;
            }

            if (Attributes != null)
            {
                context.Attributes = Attributes;
            }

            if (Attributes != null)
            {
                context.Variables = Variables;
            }

            return context;
        }
    }
}
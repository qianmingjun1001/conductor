using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 入口点
    /// </summary>
    public class EntryPoint : BaseStepDescription
    {
        public string Id { get; set; }

        /// <summary>
        /// 入口点名称
        /// </summary>
        public string Name { get; set; }

        public string StepType { get; set; }

        public string NextStepId { get; set; }

        public IDictionary<string, string> Inputs { get; set; }
    }
}
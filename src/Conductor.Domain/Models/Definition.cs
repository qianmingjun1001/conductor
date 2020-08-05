using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WorkflowCore.Models;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 定义
    /// </summary>
    public class Definition
    {
        /// <summary>
        /// 定义 Id
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// 定义版本
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Version { get; set; }

        /// <summary>
        /// 定义描述
        /// </summary>
        public string Description { get; set; }

        //public string DataType { get; set; }

        /// <summary>
        /// 默认错误处理行为
        /// </summary>
        public WorkflowErrorHandling DefaultErrorBehavior { get; set; } = WorkflowErrorHandling.Retry;

        /// <summary>
        /// 发生错误重试的间隔
        /// </summary>
        public TimeSpan? DefaultErrorRetryInterval { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 步骤
        /// </summary>
        public List<Step> Steps { get; set; } = new List<Step>();
    }
}
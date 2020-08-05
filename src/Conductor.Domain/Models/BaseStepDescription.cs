using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Conductor.Domain.Models
{
    public abstract class BaseStepDescription
    {
        /// <summary>
        /// 步骤类型
        /// </summary>
        public string Type { get; set; }
    }
}
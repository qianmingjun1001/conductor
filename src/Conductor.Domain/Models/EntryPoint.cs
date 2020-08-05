using System.ComponentModel.DataAnnotations;

namespace Conductor.Domain.Models
{
    /// <summary>
    /// 入口点
    /// </summary>
    public class EntryPoint : BaseStepDescription
    {
        /// <summary>
        /// 入口点名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 入口点路径
        /// </summary>
        [MaxLength(100)]
        public string Path { get; set; }
    }
}
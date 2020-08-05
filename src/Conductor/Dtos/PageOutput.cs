using System.Collections.Generic;

namespace Conductor.Dtos
{
    /// <summary>
    /// 分页输出
    /// </summary>
    public class PageOutput<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="count"></param>
        public PageOutput(List<T> rows, int count)
        {
            Rows = rows;
            Count = count;
        }

        /// <summary>
        /// 数据
        /// </summary>
        public List<T> Rows { get; }

        /// <summary>
        /// 总量
        /// </summary>
        public int Count { get; }
    }
}
using System;
using System.Runtime.Serialization;

namespace Conductor.Domain.Entities
{
    /// <summary>
    /// 未找到实体异常
    /// </summary>
    public class EntityNotFoundException : ApplicationException
    {
        /// <summary>
        /// 
        /// </summary>
        public EntityNotFoundException()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public EntityNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
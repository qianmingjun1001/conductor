using Conductor.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Conductor.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            _logger.LogError(ex, "Error Occured");
            context.Result = new ObjectResult(ApiResult<string>.False(999, ex.Message));
        }
    }
}
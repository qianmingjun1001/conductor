using System.Linq;
using Conductor.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Conductor.Filters
{
    /// <summary>
    /// 参数验证 Filter
    /// </summary>
    public class ModelValidateFilter : ActionFilterAttribute, IActionFilter
    {
        private readonly ILogger<ModelValidateFilter> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public ModelValidateFilter(ILogger<ModelValidateFilter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ApiResult<string> apiResult = null;
            if (context.ModelState.ErrorCount > 0)
            {
                var errors = context.ModelState.Values.SelectMany(p => p.Errors);
                var exceptionMsg = errors.Select(p => p.Exception).Select(p => p?.Message);
                var errorMsg = errors.Select(p => p.ErrorMessage);

                string msg = string.Join(',', errorMsg) + "-->" + string.Join(',', exceptionMsg);
                apiResult = ApiResult<string>.False(1, msg);
                context.Result = new ObjectResult(apiResult);
                _logger.LogInformation(msg);

                return;
            }

            if (context.ActionArguments != null && context.ActionArguments.Count > 0)
            {
                foreach (var item in context.ActionArguments)
                {
                    if (item.Value == null)
                    {
                        apiResult = ApiResult<string>.False(2, $"参数[{item.Key}]不能空");
                        context.Result = new ObjectResult(apiResult);
                        _logger.LogInformation(apiResult.Message);

                        return;
                    }
                }
            }
        }
    }
}
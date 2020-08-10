using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WorkflowCore.Interface;

namespace Conductor.DynamicRoute
{
    public class EntryPointEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFlowDefinitionService _flowDefinitionService;
        private readonly IWorkflowController _workflowController;

        public EntryPointEndpointMiddleware(
            RequestDelegate next,
            IFlowDefinitionService flowDefinitionService,
            IWorkflowController workflowController)
        {
            _next = next;
            _flowDefinitionService = flowDefinitionService;
            _workflowController = workflowController;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var path = request.Path;

            var workflowContext = await GetWorkflowContextFromRequest(request);
            if (workflowContext != null)
            {
                var definitions = await _flowDefinitionService.GetFlowDefinitionIdAndVersionByEntryPointPath(path);
                if (definitions.Count > 0)
                {
                    var builder = new StringBuilder();

                    //开启工作流
                    foreach (var definition in definitions)
                    {
                        await _workflowController.StartWorkflow(definition.id, definition.version, workflowContext);
                        builder.AppendFormat("启动工作流 [id: {0}, version: {1}] 成功", definition.id, definition.version);
                        builder.AppendLine();
                    }

                    var response = context.Response;
                    response.ContentType = "text/plain";
                    await response.WriteAsync(builder.ToString(), Encoding.UTF8);

                    return;
                }
            }


            await _next(context);
        }

        private async Task<WorkflowContext> GetWorkflowContextFromRequest(HttpRequest request)
        {
            var method = request.Method;
            if (HttpMethods.IsGet(method))
            {
                return new WorkflowContext();
            }

            if (HttpMethods.IsPost(method))
            {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    return JsonUtils.Deserialize<WorkflowContext>(await reader.ReadToEndAsync());
                }
            }

            return null;
        }
    }
}
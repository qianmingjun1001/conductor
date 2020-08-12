using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Conductor.Auth;
using Conductor.Domain.Entities;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using Conductor.Dtos;
using Conductor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;

namespace Conductor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class WorkflowController : Controller
    {
        private readonly IWorkflowController _workflowController;
        private readonly IPersistenceProvider _persistenceProvider;
        private readonly IMapper _mapper;
        private readonly IFlowDefinitionService _flowDefinitionService;

        public WorkflowController(
            IWorkflowController workflowController, IPersistenceProvider persistenceProvider, IMapper mapper, IFlowDefinitionService flowDefinitionService)
        {
            _workflowController = workflowController;
            _persistenceProvider = persistenceProvider;
            _mapper = mapper;
            _flowDefinitionService = flowDefinitionService;
        }

        /// <summary>
        /// 通过启动后工作流的实例id获取工作流实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpGet("{instanceId}")]
        [Authorize(Policy = Policies.Viewer)]
        public async Task<ApiResult<WorkflowInstance>> Get(string instanceId)
        {
            var result = await _persistenceProvider.GetWorkflowInstance(instanceId);
            if (result == null)
            {
                return ApiResult<WorkflowInstance>.False($"根据 id: {instanceId} 未查询到结果");
            }

            return ApiResult<WorkflowInstance>.True(_mapper.Map<WorkflowInstance>(result));
        }

        /// <summary>
        /// 通过工作流定义的flowId开启工作流
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("{flowId}")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<WorkflowInstance>> Post(Guid flowId, [FromBody] WorkflowContextDto data)
        {
            var definition = await CheckWorkflowDefinition(flowId);

            var instanceId = await _workflowController.StartWorkflow(definition.DefinitionId, data.ToWorkflowContext());
            var result = await _persistenceProvider.GetWorkflowInstance(instanceId);

            return ApiResult<WorkflowInstance>.True(_mapper.Map<WorkflowInstance>(result));
        }

        /// <summary>
        /// 通过工作流定义的id和版本开启工作流
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("{flowId}/{version}")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<WorkflowInstance>> Post(Guid flowId, int version, [FromBody] WorkflowContextDto data)
        {
            var definition = await CheckWorkflowDefinition(flowId, version);

            var instanceId = await _workflowController.StartWorkflow(definition.DefinitionId, definition.DefinitionVersion, data.ToWorkflowContext());
            var result = await _persistenceProvider.GetWorkflowInstance(instanceId);

            return ApiResult<WorkflowInstance>.True(_mapper.Map<WorkflowInstance>(result));
        }

        //验证工作流定义是否存在
        private async Task<FlowDefinition> CheckWorkflowDefinition(Guid flowId, int? version = null)
        {
            var definition = await _flowDefinitionService.GetFlow(flowId);

            if (definition == null || (version.HasValue && definition.DefinitionVersion != version.Value))
            {
                throw new InvalidOperationException($"flowId: {flowId}, version: {version} 的工作流定义不存在");
            }

            return definition;
        }

        /// <summary>
        /// 通过启动后工作流的实例id暂停工作流实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpPut("{instanceId}/suspend")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Suspend(string instanceId)
        {
            var result = await _workflowController.SuspendWorkflow(instanceId);

            return result ? ApiResult<string>.True($"暂停工作流 [{instanceId}] 成功") : ApiResult<string>.False($"暂停工作流 [{instanceId}] 失败");
        }

        /// <summary>
        /// 通过启动后工作流的实例id恢复工作流实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpPut("{instanceId}/resume")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Resume(string instanceId)
        {
            var result = await _workflowController.ResumeWorkflow(instanceId);
            return result ? ApiResult<string>.True($"恢复工作流 [{instanceId}] 成功") : ApiResult<string>.False($"恢复工作流 [{instanceId}] 失败");
        }

        /// <summary>
        /// 通过启动后工作流的实例id终止工作流实例
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        [HttpDelete("{instanceId}")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Terminate(string instanceId)
        {
            var result = await _workflowController.TerminateWorkflow(instanceId);
            return result ? ApiResult<string>.True($"终止工作流 [{instanceId}] 成功") : ApiResult<string>.False($"终止工作流 [{instanceId}] 失败");
        }
    }
}
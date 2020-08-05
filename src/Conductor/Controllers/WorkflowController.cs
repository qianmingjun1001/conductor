using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Conductor.Auth;
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

        public WorkflowController(IWorkflowController workflowController, IPersistenceProvider persistenceProvider, IMapper mapper)
        {
            _workflowController = workflowController;
            _persistenceProvider = persistenceProvider;
            _mapper = mapper;
        }

        /// <summary>
        /// 获取工作流实例
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Policy = Policies.Viewer)]
        public async Task<ApiResult<WorkflowInstance>> Get(string id)
        {
            var result = await _persistenceProvider.GetWorkflowInstance(id);
            if (result == null)
            {
                return ApiResult<WorkflowInstance>.False($"根据 id: {id} 未查询到结果");
            }

            return ApiResult<WorkflowInstance>.True(_mapper.Map<WorkflowInstance>(result));
        }

        /// <summary>
        /// 开启工作流
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<WorkflowInstance>> Post(string id, [FromBody] WorkflowContext data)
        {
            var instanceId = await _workflowController.StartWorkflow(id, data);
            var result = await _persistenceProvider.GetWorkflowInstance(instanceId);

            return ApiResult<WorkflowInstance>.True(_mapper.Map<WorkflowInstance>(result));
        }

        [HttpPut("{id}/suspend")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Suspend(string id)
        {
            var result = await _workflowController.SuspendWorkflow(id);

            return result ? ApiResult<string>.True($"暂停工作流 [{id}] 成功") : ApiResult<string>.False($"暂停工作流 [{id}] 失败");
        }

        [HttpPut("{id}/resume")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Resume(string id)
        {
            var result = await _workflowController.ResumeWorkflow(id);
            return result ? ApiResult<string>.True($"恢复工作流 [{id}] 成功") : ApiResult<string>.False($"恢复工作流 [{id}] 失败");
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.Controller)]
        public async Task<ApiResult<string>> Terminate(string id)
        {
            var result = await _workflowController.TerminateWorkflow(id);
            return result ? ApiResult<string>.True($"终止工作流 [{id}] 成功") : ApiResult<string>.False($"终止工作流 [{id}] 失败");
        }
    }
}
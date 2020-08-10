using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Conductor.Domain.Entities;
using Conductor.Domain.Interfaces;
using Conductor.Dtos;
using Conductor.DynamicRoute;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Conductor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowDefinitionController : Controller
    {
        private readonly IFlowDefinitionService _flowDefinitionService;
        private readonly IMapper _mapper;
        private readonly EntryPointRouteRegistry _entryPointRouteRegistry;

        public FlowDefinitionController(
            [NotNull] IFlowDefinitionService flowDefinitionService,
            [NotNull] IMapper mapper,
            [NotNull] EntryPointRouteRegistry entryPointRouteRegistry)
        {
            _flowDefinitionService = flowDefinitionService;
            _mapper = mapper;
            _entryPointRouteRegistry = entryPointRouteRegistry;
        }

        /// <summary>
        /// 创建、修改流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult<Guid>> Post([NotNull] [FromBody] FlowDefinitionInput input)
        {
            var flowId = await _flowDefinitionService.SaveFlow(input.ToFlowDefinition());

            var path = input.EntryPoint?.Path;
            if (!string.IsNullOrEmpty(path))
            {
                _entryPointRouteRegistry.RegisterRoute(path);
            }

            return ApiResult<Guid>.True(flowId);
        }

        /// <summary>
        /// 根据 id 获取流程
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        [HttpGet("{flowId}")]
        public async Task<ApiResult<FlowDefinitionOutput>> Get(Guid flowId)
        {
            if (flowId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(flowId)} 不能等于0");
            }

            var flow = await _flowDefinitionService.GetFlow(flowId);
            return ApiResult<FlowDefinitionOutput>.True(_mapper.Map<FlowDefinitionOutput>(flow));
        }

        /// <summary>
        /// 获取流程分页列表
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ApiResult<PageOutput<FlowDefinitionOutput>>> GetPaged([FromQuery] int offset, [FromQuery] int limit)
        {
            if (offset < 0)
            {
                throw new ArgumentException($"{nameof(offset)} 不能小于0");
            }

            if (limit <= 0)
            {
                throw new ArgumentException($"{nameof(limit)} 不能小于或等于0");
            }

            var (rows, count) = await _flowDefinitionService.GetFlowPaged(offset, limit);

            return ApiResult<PageOutput<FlowDefinitionOutput>>.True(
                new PageOutput<FlowDefinitionOutput>(
                    _mapper.Map<List<FlowDefinitionOutput>>(rows),
                    count
                ));
        }

        [HttpDelete("{flowId}")]
        public async Task<ApiResult<int>> Delete(Guid flowId)
        {
            if (flowId == Guid.Empty)
            {
                throw new ArgumentException($"{nameof(flowId)} 不能等于0");
            }

            await _flowDefinitionService.DeleteFlow(flowId);
            return ApiResult<int>.Empty();
        }
    }
}
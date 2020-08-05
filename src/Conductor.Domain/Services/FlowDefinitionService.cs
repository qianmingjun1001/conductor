using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Entities;
using Conductor.Domain.EventData;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Conductor.Domain.Services
{
    public class FlowDefinitionService : IFlowDefinitionService
    {
        [NotNull]
        private readonly IFlowDefinitionRepository _flowDefinitionRepository;

        [NotNull]
        private readonly IMediator _mediator;

        public FlowDefinitionService([NotNull] IFlowDefinitionRepository flowDefinitionRepository, [NotNull] IMediator mediator)
        {
            _flowDefinitionRepository = flowDefinitionRepository;
            _mediator = mediator;
        }

        public async Task<Guid> SaveFlow(FlowDefinition entity)
        {
            var definition = JsonUtils.Deserialize<Definition>(entity.Definition);
            //add
            if (entity.FlowId == Guid.Empty)
            {
                await _mediator.Publish(new CreateFlowDefinitionEventData(definition));
                return await _flowDefinitionRepository.InsertAsync(entity);
            }

            //update
            var item = await GetFlow(entity.FlowId);
            item.FlowName = entity.FlowName;
            item.Definition = entity.Definition;
            item.DefinitionId = entity.DefinitionId;
            item.DefinitionVersion = entity.DefinitionVersion;
            item.Description = entity.Description;
            item.EntryPoint = entity.EntryPoint;
            item.EntryPointPath = entity.EntryPointPath;

            await _mediator.Publish(new UpdateFlowDefinitionEventData(definition));
            await _flowDefinitionRepository.UpdateAsync(item);
            return entity.FlowId;
        }

        public async Task<FlowDefinition> GetFlow(Guid flowId)
        {
            return await _flowDefinitionRepository.GetAsync(flowId);
        }

        public async Task<(List<FlowDefinition> rows, int count)> GetFlowPaged(int offset, int limit)
        {
            return await _flowDefinitionRepository.GetPagedListAsync(offset, limit);
        }

        public async Task DeleteFlow(Guid flowId)
        {
            await _flowDefinitionRepository.DeleteAsync(flowId);
        }

        public async Task LoadDefinitionsFromStorage()
        {
            foreach (var entity in await _flowDefinitionRepository.GetListAsync())
            {
                var definition = JsonUtils.Deserialize<Definition>(entity.Definition);
                await _mediator.Publish(new LoadFlowDefinitionEventData(definition));
            }
        }
    }
}
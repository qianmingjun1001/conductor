using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

            FlowDefinition item;
            //add
            if (entity.FlowId == Guid.Empty)
            {
                item = await GetFlowByIdAndVersion(entity.DefinitionId, entity.DefinitionVersion);
                if (item != null)
                {
                    throw new InvalidOperationException($"已存在 id: {entity.DefinitionId}, version: {entity.DefinitionVersion} 的工作流定义");
                }

                await _mediator.Publish(new CreateFlowDefinitionEventData(definition));
                return await _flowDefinitionRepository.InsertAsync(entity);
            }

            //update
            item = await GetFlow(entity.FlowId);
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

        public async Task<FlowDefinition> GetFlowByIdAndVersion(string definitionId, int? version = null)
        {
            Expression<Func<FlowDefinition, bool>> predicate = p => p.DefinitionId == definitionId;
            predicate = predicate.AndIf(version.HasValue, p => p.DefinitionVersion == version.Value);

            return await _flowDefinitionRepository.GetFirstOrDefault(predicate);
        }

        public async Task<List<(string id, int version)>> GetFlowDefinitionIdAndVersionByEntryPointPath(string path)
        {
            return (await _flowDefinitionRepository.GetListAsync(p => p.EntryPointPath == path, p => new {p.DefinitionId, p.DefinitionVersion}))
                .Select(p => (p.DefinitionId, p.DefinitionVersion))
                .ToList();
        }

        public async Task<List<string>> GetAllEntryPointPath()
        {
            var list = new List<string>();
            foreach (var flowDefinition in await _flowDefinitionRepository.GetListAsync(p => p.EntryPointPath.Length > 0, p => new {p.EntryPointPath}))
            {
                if (!list.Contains(flowDefinition.EntryPointPath))
                {
                    list.Add(flowDefinition.EntryPointPath);
                }
            }

            return list;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Conductor.Domain.Entities;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using Conductor.Domain.Utils;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Conductor.Storage.SqlServer.Services
{
    public class FlowDefinitionRepository : IFlowDefinitionRepository, IDefinitionRepository
    {
        [NotNull]
        private readonly WorkflowDbContextFactory _workflowDbContextFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workflowDbContextFactory"></param>
        public FlowDefinitionRepository([NotNull] WorkflowDbContextFactory workflowDbContextFactory)
        {
            _workflowDbContextFactory = workflowDbContextFactory;
        }

        private WorkflowDbContext ConstructDbContext()
        {
            return _workflowDbContextFactory.Build();
        }

        public async Task<Guid> InsertAsync(FlowDefinition entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            entity.CreateTime = DateTime.Now;
            entity.Creator = null;

            using (var context = ConstructDbContext())
            {
                await context.FlowDefinitions.AddAsync(entity);
                await context.SaveChangesAsync();
            }

            return entity.FlowId;
        }

        public async Task UpdateAsync(FlowDefinition entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            using (var context = ConstructDbContext())
            {
                context.FlowDefinitions.Update(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<FlowDefinition> GetAsync(Guid id)
        {
            using (var context = ConstructDbContext())
            {
                var item = await context.FlowDefinitions.FindAsync(id);
                if (item == null)
                {
                    throw new EntityNotFoundException($"根据 id: {id} 未查询到实体");
                }

                return item;
            }
        }

        public async Task<List<FlowDefinition>> GetListAsync(Expression<Func<FlowDefinition, bool>> predicate = null)
        {
            using (var context = ConstructDbContext())
            {
                IQueryable<FlowDefinition> queryable = context.FlowDefinitions;
                if (predicate != null)
                {
                    queryable = queryable.Where(predicate);
                }

                return await queryable.ToListAsync();
            }
        }

        public async Task<(List<FlowDefinition> rows, int count)> GetPagedListAsync(int offset, int limit, Expression<Func<FlowDefinition, bool>> predicate = null)
        {
            using (var context = ConstructDbContext())
            {
                IQueryable<FlowDefinition> queryable = context.FlowDefinitions;
                if (predicate != null)
                {
                    queryable = queryable.Where(predicate);
                }

                return (
                    await queryable.Skip(offset).Take(limit).ToListAsync(),
                    await queryable.CountAsync()
                );
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await GetAsync(id);

            using (var context = ConstructDbContext())
            {
                context.FlowDefinitions.Remove(item);
                await context.SaveChangesAsync();
            }
        }

        #region IDefinitionRepository

        public IEnumerable<Definition> GetAll()
        {
            using (var context = ConstructDbContext())
            {
                return context.FlowDefinitions.Select(p => p.Definition).Select(p => JsonUtils.Deserialize<Definition>(p));
            }
        }

        public Definition Find(string workflowId)
        {
            using (var context = ConstructDbContext())
            {
                var item = context.FlowDefinitions.FirstOrDefault(p => p.DefinitionId == workflowId);
                return item == null ? null : JsonUtils.Deserialize<Definition>(item.Definition);
            }
        }

        public Definition Find(string workflowId, int version)
        {
            using (var context = ConstructDbContext())
            {
                var item = context.FlowDefinitions.FirstOrDefault(p => p.DefinitionId == workflowId && p.DefinitionVersion == version);
                return item == null ? null : JsonUtils.Deserialize<Definition>(item.Definition);
            }
        }

        public int? GetLatestVersion(string workflowId)
        {
            using (var context = ConstructDbContext())
            {
                return context.FlowDefinitions
                    .OrderByDescending(p => p.DefinitionVersion)
                    .Select(p => p.DefinitionVersion)
                    .FirstOrDefault();
            }
        }

        public void Save(Definition definition)
        {
            throw new InvalidOperationException("不支持改方法");
        }

        #endregion
    }
}
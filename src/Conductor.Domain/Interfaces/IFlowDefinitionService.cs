using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Entities;

namespace Conductor.Domain.Interfaces
{
    public interface IFlowDefinitionService
    {
        /// <summary>
        /// 保存流程
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<Guid> SaveFlow(FlowDefinition entity);

        /// <summary>
        /// 获取流程
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        Task<FlowDefinition> GetFlow(Guid flowId);

        /// <summary>
        /// 获取分页流程列表
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<(List<FlowDefinition> rows, int count)> GetFlowPaged(int offset, int limit);

        /// <summary>
        /// 删除流程
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        Task DeleteFlow(Guid flowId);

        /// <summary>
        /// 从存储中加载流程定义
        /// </summary>
        /// <returns></returns>
        Task LoadDefinitionsFromStorage();
    }
}
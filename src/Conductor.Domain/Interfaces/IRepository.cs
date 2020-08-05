using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Conductor.Domain.Entities;

namespace Conductor.Domain.Interfaces
{
    /// <summary>
    /// 简单的 Repository 基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 创建实体
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<Guid> InsertAsync(TEntity entity);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity"></param>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// 通过 id 获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> GetAsync(Guid id);

        /// <summary>
        /// 获取第一个，如果获取不到返回 null
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<TEntity> GetFirstOrDefault(Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 获取分页实体列表
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<(List<TEntity> rows, int count)> GetPagedListAsync(int offset, int limit, Expression<Func<TEntity, bool>> predicate = null);

        /// <summary>
        /// 根据 id 删除实体
        /// </summary>
        /// <param name="id"></param>
        Task DeleteAsync(Guid id);
    }
}
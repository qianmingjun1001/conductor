using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Conductor.Domain.Utils
{
    /// <summary>
    /// 表达式树重新绑定
    /// </summary>
    internal class ParameterRebinder : System.Linq.Expressions.ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> map;

        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
        {
            return new ParameterRebinder(map).Visit(exp);
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            ParameterExpression replacement;
            if (map.TryGetValue(p, out replacement))
            {
                p = replacement;
            }

            return base.VisitParameter(p);
        }
    }

    /// <summary>
    /// 表达式树拼接类
    /// </summary>
    public static class PredicateBuilder
    {
        /// <summary>
        /// 返回true表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        /// <summary>
        /// 返回false表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        /// <summary>
        /// 使用AndAlso拼接两个表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        /// <summary>
        /// 条件判断，使用AndAlso拼接两个表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
        {
            if (condition)
                return first.Compose(second, Expression.AndAlso);
            return first;
        }

        /// <summary>
        /// 使用OrElse拼接两个表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        /// <summary>
        /// 条件判断,使用OrElse拼接两个表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="condition"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> first, bool condition, Expression<Func<T, bool>> second)
        {
            if (condition)
                return first.Compose(second, Expression.OrElse);
            return first;
        }

        /// <summary>
        /// 拼接两个表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        internal static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            // build parameter map (from parameters of second to parameters of first)
            var map = first.Parameters.Select((f, i) => new {f, s = second.Parameters[i]}).ToDictionary(p => p.s, p => p.f);
            // replace parameters in the second lambda expression with parameters from the first
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            // apply composition of lambda expression bodies to parameters from the first expression 
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using JetBrains.Annotations;

namespace Conductor.Domain.Scripting.ExpressionTree
{
    public class ScriptEngineHost : IScriptEngineHost
    {
        public void Execute(Resource resource, IDictionary<string, object> inputs)
        {
            throw new NotImplementedException();
        }

        public dynamic EvaluateExpression([NotNull] string expression, [NotNull] IDictionary<string, object> inputs)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));

            var parameters = new List<ParameterExpression>();
            
            //data, payload, attributes, vars
            var dataType = typeof(WorkflowContext);
            var attribute = dataType.GetCustomAttribute<KeywordsAttribute>();
            if (attribute != null)
            {
                var dataParameter = Expression.Parameter(dataType, attribute.Name);
                parameters.Add(dataParameter);
            }

            foreach (var property in dataType.GetProperties())
            {
                attribute = property.GetCustomAttribute<KeywordsAttribute>();
                if (attribute != null)
                {
                    var propertyParameter = Expression.Parameter(property.PropertyType, attribute.Name);
                    parameters.Add(propertyParameter);
                }
            }

            //environment
            parameters.Add(Expression.Parameter(typeof(IDictionary), "environment"));


            //customize
            //[parameter, type]
            foreach (var input in inputs)
            {
                if (input.Value is Type type)
                {
                    var parameter = Expression.Parameter(type, input.Key);
                    parameters.Add(parameter);
                }
            }

            return DynamicExpressionParser.ParseLambda(parameters.ToArray(), typeof(object), expression);
        }

        public T EvaluateExpression<T>(string expression, IDictionary<string, object> inputs)
        {
            throw new NotImplementedException();
        }
    }
}
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
            throw new NotImplementedException();
        }

        public dynamic EvaluateExpression(string expression, object pData, IDictionary<string, object> inputs)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));

            //data, payload, attributes, vars, environment
            var dataType = typeof(WorkflowContext);
            var data = (WorkflowContext) pData;
            var parameters = new List<ParameterExpression>()
            {
                Expression.Parameter(dataType, "data"),
                Expression.Parameter(data.GetPayloadType(), "payload"),
                Expression.Parameter(typeof(Dictionary<string, string>), "attributes"),
                Expression.Parameter(typeof(Dictionary<string, string>), "vars"),
                Expression.Parameter(typeof(IDictionary), "environment")
            };

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
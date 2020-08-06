using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using WorkflowCore.Interface;

namespace Conductor.Domain.ReplacedServices
{
    public class ReplacedExpressionEvaluator : IExpressionEvaluator
    {
        private readonly IScriptEngineHost _scriptHost;

        public ReplacedExpressionEvaluator(IScriptEngineHost scriptHost)
        {
            _scriptHost = scriptHost;
        }

        public object EvaluateExpression(string sourceExpr, object pData, IStepExecutionContext pContext)
        {
            var lambda = (LambdaExpression) _scriptHost.EvaluateExpression(sourceExpr, pData, new Dictionary<string, object>()
            {
                ["context"] = typeof(IStepExecutionContext)
            });

            var data = (WorkflowContext) pData;

            return lambda.Compile().DynamicInvoke(data, data.Payload, data.Attributes, data.Variables, Environment.GetEnvironmentVariables(), pContext);
        }

        public object EvaluateExpression(string sourceExpr, object pData, object pStep)
        {
            var lambda = (LambdaExpression) _scriptHost.EvaluateExpression(sourceExpr, pData, new Dictionary<string, object>()
            {
                ["step"] = pStep.GetType()
            });

            var data = (WorkflowContext) pData;

            return lambda.Compile().DynamicInvoke(data, data.Payload, data.Attributes, data.Variables, Environment.GetEnvironmentVariables(), pStep);
        }

        public object EvaluateExpression(string sourceExpr, [NotNull] IDictionary<string, object> parameteters)
        {
            throw new NotImplementedException();
        }

        public bool EvaluateOutcomeExpression(string sourceExpr, object pData, object outcome)
        {
            var inputs = new Dictionary<string, object>();
            if (outcome != null)
            {
                inputs["outcome"] = outcome.GetType();
            }

            var lambda = (LambdaExpression) _scriptHost.EvaluateExpression(sourceExpr, pData, inputs);

            var data = (WorkflowContext) pData;

            var args = new List<object>()
            {
                data, data.Payload, data.Attributes, data.Variables, Environment.GetEnvironmentVariables()
            };
            if (outcome != null)
            {
                args.Add(outcome);
            }

            return Convert.ToBoolean(lambda.Compile().DynamicInvoke(args.ToArray()));
        }
    }
}
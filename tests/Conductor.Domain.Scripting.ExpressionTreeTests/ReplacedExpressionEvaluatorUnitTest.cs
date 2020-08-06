using System.Collections.Generic;
using System.Dynamic;
using Conductor.Domain.Interfaces;
using Conductor.Domain.Models;
using Conductor.Domain.ReplacedServices;
using Conductor.Domain.Scripting.ExpressionTree;
using NUnit.Framework;
using WorkflowCore.Models;

namespace Conductor.Domain.Scripting.ExpressionTreeTests
{
    [TestFixture]
    public class ReplacedExpressionEvaluatorUnitTest
    {
        private WorkflowContext _workflowContext;
        private ReplacedExpressionEvaluator _expressionEvaluator;

        [SetUp]
        public void SetUp()
        {
            dynamic any = new ExpandoObject();
            any.payload1 = "p1";
            any.payload2 = "p2";

            _workflowContext = new WorkflowContext()
            {
                Any = any,
                Attributes = new Dictionary<string, string>()
                {
                    ["attr1"] = "a1",
                    ["attr2"] = "a2"
                },
                Variables = new Dictionary<string, string>()
                {
                    ["var1"] = "v1",
                    ["var2"] = "v2"
                }
            };

            _expressionEvaluator = new ReplacedExpressionEvaluator(new ScriptEngineHost());
        }

        [Test]
        public void EvaluateExpressionTest()
        {
            object o = _expressionEvaluator.EvaluateExpression(
                "data.Payload.payload1 + payload.payload2 + data.Attributes[\"attr1\"] + attributes[\"attr2\"] + data.Variables[\"var1\"] + vars[\"var2\"]",
                _workflowContext,
                new StepExecutionContext());
            Assert.AreEqual("p1p2a1a2v1v2", o);
        }
    }
}
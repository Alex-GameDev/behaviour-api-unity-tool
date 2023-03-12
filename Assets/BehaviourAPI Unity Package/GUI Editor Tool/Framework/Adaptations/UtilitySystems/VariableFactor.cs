using BehaviourAPI.Core;
using BehaviourAPI.UnityExtensions;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class VariableFactor : UtilitySystems.VariableFactor
    {
        public ContextualSerializedFloatFunction variableFunction;

        public override void SetExecutionContext(ExecutionContext context)
        {
            var unityContext = (UnityExecutionContext)context;
            if (unityContext != null)
            {
                variableFunction.SetContext(unityContext);
                Variable = variableFunction.GetFunction();
            }
            else
            {
                Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
            }
        }
    }
}

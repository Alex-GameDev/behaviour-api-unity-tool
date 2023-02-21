using BehaviourAPI.Core;
using BehaviourAPI.UtilitySystems;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ContextCustomFunctionFactor : FunctionFactor
    {
        private UnityExecutionContext _context;

        public ContextualSerializedFloatFloatFunction function;

        public override void SetExecutionContext(ExecutionContext context)
        {
            _context = (UnityExecutionContext)context;
            if (_context == null) Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
        }

        protected override float Evaluate(float childUtility) => function.GetFunction()?.Invoke(_context, childUtility) ?? 0f;
    }
}

using BehaviourAPI.Core;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ContextVariableFactor : UtilitySystems.VariableFactor
    {
        private UnityExecutionContext _context;

        public ContextualSerializedFloatFunction variableFunction;

        public override void SetExecutionContext(ExecutionContext context)
        {
            _context = (UnityExecutionContext)context;
            if (_context == null) Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
        }

        protected override float ComputeUtility()
        {
            Utility = variableFunction.GetFunction()?.Invoke(_context) ?? min;
            Utility = (Utility - min) / (max - min);
            return Mathf.Clamp01(Utility);
        }
    }
}

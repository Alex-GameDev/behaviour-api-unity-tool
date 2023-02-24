using BehaviourAPI.Core;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ContextVariableFactor : UtilitySystems.VariableFactor
    {
        private UnityExecutionContext _context;

        public string ComponentName;

        public ContextualSerializedFloatFunction variableFunction;

        public override void SetExecutionContext(ExecutionContext context)
        {
            _context = (UnityExecutionContext)context;
            if (_context != null)
            {
                variableFunction.SetContext(_context);
            }
            else
            {
                Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
            }
        }

        protected override float ComputeUtility()
        {
            Utility = variableFunction.GetFunction()?.Invoke() ?? min;
            Utility = (Utility - min) / (max - min);
            return Mathf.Clamp01(Utility);
        }
    }
}

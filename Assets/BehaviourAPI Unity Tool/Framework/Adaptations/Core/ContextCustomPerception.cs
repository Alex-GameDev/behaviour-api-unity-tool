using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ContextCustomPerception : Perception
    {
        private UnityExecutionContext _context;

        public ContextualSerializedAction init;
        public ContextualSerializedBoolFunction check;
        public ContextualSerializedAction reset;

        public override void SetExecutionContext(ExecutionContext context)
        {
            _context = (UnityExecutionContext)context;
            if (_context == null) Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
        }

        public override void Initialize() => init.GetFunction()?.Invoke(_context);
        public override void Reset() => reset.GetFunction()?.Invoke(_context);
        public override bool Check() => check.GetFunction()?.Invoke(_context) ?? false;
    }
}

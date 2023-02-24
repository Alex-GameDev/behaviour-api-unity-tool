using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;
using UnityEngine.PlayerLoop;

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
            if (_context != null)
            {
                init.SetContext(_context);
                check.SetContext(_context);
                reset.SetContext(_context);
            }
            else
            {
                Debug.LogError("Context perception need an UnityExecutionContext to work");
            }
        }

        public override void Initialize() => init.GetFunction()?.Invoke();
        public override void Reset() => reset.GetFunction()?.Invoke();
        public override bool Check() => check.GetFunction()?.Invoke() ?? false;
    }
}

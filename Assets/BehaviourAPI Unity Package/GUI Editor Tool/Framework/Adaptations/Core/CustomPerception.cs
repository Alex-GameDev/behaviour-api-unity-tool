using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Perceptions;
    using UnityExtensions;

    public class CustomPerception : Perception
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

        public override object Clone()
        {
            var copy = (CustomPerception)base.Clone();
            copy.init = (ContextualSerializedAction)init?.Clone();
            copy.check = (ContextualSerializedBoolFunction)check?.Clone();
            copy.reset = (ContextualSerializedAction)reset?.Clone();
            return copy;
        }
    }
}

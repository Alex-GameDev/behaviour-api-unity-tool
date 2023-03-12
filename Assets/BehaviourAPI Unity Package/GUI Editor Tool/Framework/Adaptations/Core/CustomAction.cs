using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Actions;
    using UnityExtensions;


    public class CustomAction : Action
    {
        private UnityExecutionContext _context;

        public ContextualSerializedAction start;
        public ContextualSerializedStatusFunction update;
        public ContextualSerializedAction stop;

        public override void SetExecutionContext(ExecutionContext context)
        {
            _context = (UnityExecutionContext)context;
            if (_context != null)
            {
                start.SetContext(_context);
                update.SetContext(_context);
                stop.SetContext(_context);
            }
            else
            {
                Debug.LogError("Context action need an UnityExecutionContext to work");
            }
        }

        public override void Start() => start.GetFunction()?.Invoke();

        public override void Stop() => stop.GetFunction()?.Invoke();

        public override Status Update() => update.GetFunction()?.Invoke() ?? Status.Running;

        public override object Clone()
        {
            var copy = (CustomAction)base.Clone();
            //copy.start = (ContextualSerializedAction)start?.Clone();
            //copy.update = (ContextualSerializedStatusFunction)update?.Clone();
            //copy.stop = (ContextualSerializedAction)stop?.Clone();
            return copy;
        }
    }
}

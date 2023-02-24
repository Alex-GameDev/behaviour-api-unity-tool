using BehaviourAPI.Core;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;


namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ContextCustomAction : Action
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


    }
}

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
            if (_context == null) Debug.LogError("Context Variable factor need an UnityExecutionContext to work");
        }

        public override void Start() => start.GetFunction()?.Invoke(_context);

        public override void Stop() => stop.GetFunction()?.Invoke(_context);

        public override Status Update() => update.GetFunction()?.Invoke(_context) ?? Status.Running;


    }
}

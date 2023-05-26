using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.UnityToolkit
{
    public abstract class UnityAction : Action, ITaskDisplayable
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity Action";

        public sealed override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        protected virtual void OnSetContext()
        {
            return;
        }
    }
}

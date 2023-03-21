using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.UnityExtensions
{
    public abstract class UnityAction : Action
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity Action";

        public override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        protected virtual void OnSetContext()
        {
        }
    }
}

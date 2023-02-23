using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public abstract class UnityAction : Action
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity ActionReference";

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

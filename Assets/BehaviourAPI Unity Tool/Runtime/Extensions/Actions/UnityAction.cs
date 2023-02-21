using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public abstract class UnityAction : Action
    {
        protected ExecutionContext context;
        public virtual string DisplayInfo => "Unity Action";

        public override void SetExecutionContext(ExecutionContext context)
        {
            this.context = context;
        }
    }
}

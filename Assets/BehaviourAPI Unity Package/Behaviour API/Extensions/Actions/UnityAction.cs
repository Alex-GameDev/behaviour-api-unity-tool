using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;

namespace BehaviourAPI.UnityExtensions
{
    public abstract class UnityAction : Action
    {
        protected UnityExecutionContext context;
        public virtual string DisplayInfo => "Unity Action";

        public override void Pause()
        {
            throw new System.NotImplementedException();
        }

        public sealed override void SetExecutionContext(ExecutionContext context)
        {
            this.context = (UnityExecutionContext)context;
            OnSetContext();
        }

        public override void Start()
        {
            return;
        }

        public override void Stop()
        {
            return;
        }

        public override void Unpause()
        {
            return;
        }

        protected virtual void OnSetContext()
        {
            return;
        }
    }
}

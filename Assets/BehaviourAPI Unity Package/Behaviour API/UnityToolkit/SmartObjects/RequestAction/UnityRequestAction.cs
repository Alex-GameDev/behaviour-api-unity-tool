using BehaviourAPI.Core;
using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit
{
    public abstract class UnityRequestAction : RequestAction<SmartAgent>, ITaskDisplayable
    {
        protected new UnityExecutionContext context;

        public virtual string DisplayInfo => "Request action";

        protected UnityRequestAction()
        {
        }

        protected UnityRequestAction(SmartAgent agent) : base(agent)
        {
        }

        public sealed override void SetExecutionContext(ExecutionContext ctx)
        {
            base.SetExecutionContext(ctx);
            context = (UnityExecutionContext)ctx;

            if (Agent == null)
                Agent = context.SmartAgent;

            OnSetContext(context);
        }

        protected virtual void OnSetContext(UnityExecutionContext context)
        {
        }
    }
}

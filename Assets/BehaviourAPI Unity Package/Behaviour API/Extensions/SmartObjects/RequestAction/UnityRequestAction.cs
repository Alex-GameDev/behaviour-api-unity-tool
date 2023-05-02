using BehaviourAPI.Core;
using BehaviourAPI.SmartObjects;
using BehaviourAPI.Unity.SmartObjects;

namespace BehaviourAPI.UnityExtensions
{
    public abstract class UnityRequestAction : RequestAction<SmartAgent>
    {
        protected UnityExecutionContext context;

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

            if (m_Agent == null)
                m_Agent = context.GameObject.GetComponent<SmartAgent>();

            OnSetContext(context);
        }

        protected virtual void OnSetContext(UnityExecutionContext context)
        {
        }

        protected sealed override ISmartObject<SmartAgent> FindSmartObject(SmartAgent agent) => GetSmartObject(agent);


        protected abstract SmartObject GetSmartObject(SmartAgent agent);
    }
}

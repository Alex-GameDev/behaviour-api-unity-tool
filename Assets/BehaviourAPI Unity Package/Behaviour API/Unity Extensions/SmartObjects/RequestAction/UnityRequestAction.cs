using BehaviourAPI.Core;
using BehaviourAPI.SmartObjects;
using BehaviourAPI.Unity.SmartObjects;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
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

            if (agent == null)
                agent = context.SmartAgent;

            OnSetContext(context);
        }

        protected virtual void OnSetContext(UnityExecutionContext context)
        {
        }

        protected sealed override ISmartObject<SmartAgent> FindSmartObject(SmartAgent agent)
        {
            var obj = GetSmartObject(agent);
            Debug.Log("Smart object selected", obj.gameObject);
            return obj;
        }


        protected abstract SmartObject GetSmartObject(SmartAgent agent);
    }
}

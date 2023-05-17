using BehaviourAPI.Core;
using BehaviourAPI.SmartObjects;
using BehaviourAPI.Unity.SmartObjects;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public abstract class UnityRequestAction : RequestAction<SmartAgent>
    {
        protected new UnityExecutionContext context;

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

        protected sealed override ISmartObject<SmartAgent> FindSmartObject(SmartAgent agent)
        {
            var obj = GetSmartObject(agent);
            Debug.Log("Smart object selected", obj.gameObject);
            return obj;
        }


        protected abstract SmartObject GetSmartObject(SmartAgent agent);
    }
}

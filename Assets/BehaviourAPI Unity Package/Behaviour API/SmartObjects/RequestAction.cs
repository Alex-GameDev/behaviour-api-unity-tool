namespace BehaviourAPI.SmartObjects
{
    using Core;
    using Core.Actions;
    using UnityEngine;

    /// <summary>   
    /// Action that request a behaviour to a smart object. 
    /// </summary>
    public abstract class RequestAction<TAgent> : Action where TAgent : ISmartAgent
    {
        /// <summary> 
        /// The current interaction that this action is executing. 
        /// </summary>
        protected SmartInteraction<TAgent> m_CurrentInteraction;

        /// <summary> 
        /// The agent used in the interactions. 
        /// </summary>
        protected TAgent m_Agent;


        private ExecutionContext m_Context;
        /// <summary> 
        /// Specialized constructor for use only by derived class. 
        /// </summary>
        /// <param name="agent"> The agent used in the interactions. </param>
        protected RequestAction(TAgent agent)
        {
            m_Agent = agent;
        }

        /// <summary> 
        /// Specialized default constructor for use only by derived class. 
        /// </summary>
        protected RequestAction()
        {
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            m_Context = context;
        }

        /// <summary>   
        /// Searches for a smart interaction to execute it. 
        /// </summary>
        /// <returns> The found interaction. </returns>
        protected abstract ISmartObject<TAgent> FindSmartObject(TAgent agent);

        /// <summary>   
        /// <inheritdoc/>
        /// Searches for an interaction and initializes it. If it doesn't find any, it will return failure on the next <see cref="Update"/> execution.
        ///  </summary>
        public override void Start()
        {
            ISmartObject<TAgent> obj = FindSmartObject(m_Agent);

            if (obj != null && obj.ValidateAgent(m_Agent))
            {
                m_CurrentInteraction = obj.RequestInteraction(m_Agent);

                if (m_CurrentInteraction != null)
                {
                    m_CurrentInteraction.SmartObject.InitInteraction(m_Agent);
                    m_CurrentInteraction.Action.SetExecutionContext(m_Context);
                    m_CurrentInteraction.Action.Start();
                }
            }
            else if (!obj.ValidateAgent(m_Agent))
                Debug.Log("obj is null or validate is false");
        }

        /// <summary>   
        /// <inheritdoc/>
        /// If has an active interaction, stops it and then discards it.
        ///  </summary>
        public override void Stop()
        {
            if (m_CurrentInteraction != null)
            {
                m_CurrentInteraction.Action.Stop();
                //TODO: Notificar al objeto que el agente lo ha liberado.
                m_CurrentInteraction.SmartObject.ReleaseInteraction(m_Agent);
                m_CurrentInteraction = null;
            }
        }

        /// <summary>   
        /// <inheritdoc/>
        /// If has an active interaction, stops it and then discards it.
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override Status Update()
        {
            if (m_CurrentInteraction != null)
            {
                var status = m_CurrentInteraction.Action.Update();
                if (status == Status.Success)
                {
                    foreach (var capabilityName in m_CurrentInteraction.SmartObject.GetCapabilities())
                    {
                        m_Agent.CoverNeed(capabilityName, m_CurrentInteraction.SmartObject.GetCapabilityValue(capabilityName));
                    }
                }

                if (status != Status.Running)
                    m_CurrentInteraction.SmartObject.OnComplete(m_Agent, status);

                return status;
            }
            else
            {
                return Status.Failure;
            }
        }
    }
}

﻿namespace BehaviourAPI.SmartObjects
{
    using Core;
    using Core.Actions;

    /// <summary>   
    /// Action that request a behaviour to a smart object. 
    /// </summary>
    public abstract class RequestAction<T> : Action where T : ISmartAgent
    {
        /// <summary> 
        /// The current interaction that this action is executing. 
        /// </summary>
        SmartInteraction<T> m_CurrentInteraction;

        /// <summary> 
        /// The agent used in the interactions. 
        /// </summary>
        public T agent { get; set; }

        /// <summary>
        /// The context that the request action will propagate throught the provided actions.
        /// </summary>
        protected ExecutionContext context;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected RequestAction()
        {
        }

        /// <summary> 
        /// Specialized constructor for use only by derived class. 
        /// </summary>
        /// <param name="agent"> The agent used in the interactions. </param>
        protected RequestAction(T agent)
        {
            this.agent = agent;
        }

        /// <summary>   
        /// Searches for a smart interaction to execute it. 
        /// </summary>
        /// <returns> The found interaction. </returns>
        protected abstract ISmartObject<T> FindSmartObject(T agent);

        /// <summary> 
        /// Request interaction. 
        /// </summary>
        /// <param name="smartObject"> The smart object. </param>
        /// <param name="agent"> The agent used in the interactions. </param>
        /// <returns> A SmartInteraction. </returns>
        protected virtual string GetInteractionName()
        {
            return null;
        }

        /// <summary>   
        /// <inheritdoc/>
        /// Searches for an interaction and initializes it. If it doesn't find any, it will return failure on the next <see cref="Update"/> execution.
        ///  </summary>
        public override void Start()
        {
            if (agent == null)
                throw new MissingAgentException<T>(this, "Can't send request to a smart object without smart agent");

            ISmartObject<T> obj = FindSmartObject(agent);

            if (obj != null && obj.ValidateAgent(agent))
            {
                var interactionName = GetInteractionName();
                m_CurrentInteraction = obj.RequestInteraction(agent, interactionName);

                if (m_CurrentInteraction != null)
                {
                    m_CurrentInteraction.SmartObject.OnInitInteraction(agent);

                    if (context != null)
                    {
                        m_CurrentInteraction.Action.SetExecutionContext(context);
                    }
                    m_CurrentInteraction.Action.Start();
                }
            }
        }

        /// <summary>   
        /// <inheritdoc/>
        /// If has an active interaction, stops it and then discards it.
        ///  </summary>
        public override void Stop()
        {
            if (agent == null)
                throw new MissingAgentException<T>(this, "Can't release interaction to a smart object without smart agent");

            if (m_CurrentInteraction != null)
            {
                m_CurrentInteraction.Action.Stop();
                m_CurrentInteraction.SmartObject.OnReleaseInteraction(agent);
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
            if (agent == null)
                throw new MissingAgentException<T>(this, "Can't update request to a smart object without smart agent");

            if (m_CurrentInteraction != null)
            {
                var status = m_CurrentInteraction.Action.Update();

                if (status == Status.Success)
                {
                    foreach (var capabilityName in m_CurrentInteraction.SmartObject.GetCapabilities())
                    {
                        agent.CoverNeed(capabilityName, m_CurrentInteraction.SmartObject.GetCapabilityValue(capabilityName));
                    }
                }

                if (status != Status.Running)
                    m_CurrentInteraction.SmartObject.OnCompleteInteraction(agent, status);

                return status;
            }
            else
            {
                return Status.Failure;
            }
        }

        public override void Pause()
        {
            if (m_CurrentInteraction != null)
            {
                if (agent == null)
                    throw new MissingAgentException<T>(this, "Can't pause interaction to a smart object without smart agent");

                m_CurrentInteraction.Action.Pause();
                m_CurrentInteraction.SmartObject.OnPauseInteraction(agent);
            }
        }

        public override void Unpause()
        {
            if (m_CurrentInteraction != null)
            {
                if (agent == null)
                    throw new MissingAgentException<T>(this, "Can't unpause interaction to a smart object without smart agent");

                m_CurrentInteraction.Action.Unpause();
                m_CurrentInteraction.SmartObject.OnUnpauseInteraction(agent);
            }
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            this.context = context;
        }
    }
}

using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    using BehaviourAPI.SmartObjects;
    using Core.Actions;
    using UnityToolkit;

    /// <summary> 
    /// A subtype of the SmartObject that the agent navigates to in order to use it. 
    /// Also, the item can only be used by one agent at a time.
    /// </summary>

    public abstract class DirectSmartObject : SimpleSmartObject
    {
        [Tooltip("The target where the agent must be placed to use the item")]
        [SerializeField] protected Transform _placeTarget;

        /// <summary> 
        /// Gets or sets the owner. 
        /// The current agent using the object. If the property has value
        /// the object is not selectable for other agents.
        /// </summary>
        /// <value> The owner. </value>
        public SmartAgent Owner { get; protected set; }

        public override bool ValidateAgent(SmartAgent agent)
        {
            return Owner == null;
        }

        protected sealed override Action GenerateAction(SmartAgent agent, RequestData requestData)
        {
            // Simple way to make an action sequence
            SequenceAction sequence = new SequenceAction();

            // First step: move to smart object
            sequence.SubActions.Add(new WalkAction(_placeTarget.position));

            // Second step: use the smart object
            sequence.SubActions.Add(GetUseAction(agent, requestData));

            return sequence;
        }

        protected abstract Action GetUseAction(SmartAgent agent, RequestData requestData);

        protected override void SetInteractionEvents(SmartInteraction interaction, SmartAgent agent)
        {
            interaction.OnInitialize += () => OnInitInteraction(agent);
            interaction.OnRelease += () => OnReleaseInteraction(agent);
        }

        void OnInitInteraction(SmartAgent agent)
        {
            if (_registerOnManager)
                SmartObjectManager.Instance?.UnregisterSmartObject(this);

            if (Owner != null)
                Debug.LogError("Error: Trying to init a smart object interaction when the agent is not null", this);

            Owner = agent;
        }


        void OnReleaseInteraction(SmartAgent agent)
        {
            if (_registerOnManager)
                SmartObjectManager.Instance?.RegisterSmartObject(this);

            if (Owner != agent)
                Debug.LogError("Error: Trying to release a smart object interaction from an Agent that is not the owner", this);

            Owner = null;
        }
    }
}

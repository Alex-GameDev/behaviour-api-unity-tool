using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    using BehaviourTrees;
    using Core.Actions;
    using SmartObjects;
    using UnityExtensions;

    /// <summary> 
    /// A subtype of the SmartObject that the agent navigates to in order to use it. 
    /// Also, the item can only be used by one agent at a time.
    /// </summary>

    public abstract class DirectSmartObject : SmartObject
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

        protected sealed override Action GetRequestedAction(SmartAgent agent)
        {
            BehaviourTree bt = new BehaviourTree();

            Action placeAction = new FunctionalAction(() => agent.Movement.Move(_placeTarget.position, _placeTarget.rotation));
            var movementNode = bt.CreateLeafNode(placeAction);

            Action useAction = GetUseAction(agent);
            var actionNode = bt.CreateLeafNode(useAction);

            var seq = bt.CreateComposite<SequencerNode>(false, movementNode, actionNode);
            bt.SetRootNode(seq);
            return new SubsystemAction(bt);
        }

        protected abstract Action GetUseAction(SmartAgent agent);

        public override void InitInteraction(SmartAgent agent)
        {
            if (_registerOnManager)
                SmartObjectManager.Instance?.UnregisterSmartObject(this);

            if (Owner != null)
                Debug.LogError("Error: Trying to init a smart object interaction when the agent is not null", this);

            Owner = agent;
        }


        public override void ReleaseInteraction(SmartAgent agent)
        {
            if (_registerOnManager)
                SmartObjectManager.Instance?.RegisterSmartObject(this);

            if (Owner != agent)
                Debug.LogError("Error: Trying to release a smart object interaction from an Agent that is not the owner", this);

            Owner = null;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.SmartObjects
{
    using BehaviourAPI.SmartObjects;
    using BehaviourTrees;
    using Core.Actions;
    using UnityExtensions;

    /// <summary> 
    /// Unity component to implement a Smart Object.
    /// </summary>

    public abstract class SmartObject : MonoBehaviour, ISmartObject<SmartAgent>
    {
        [SerializeField] SmartObjectSettings _config;

        /// <summary> 
        /// Flag to register the smart object in the <see cref="SmartObjectManager"/>. 
        /// </summary>
        public bool _registerOnManager = true;

        void OnEnable()
        {
            if (_registerOnManager)
                SmartObjectManager.Instance.RegisterSmartObject(this);
        }

        void OnDisable()
        {
            if (_registerOnManager)
                SmartObjectManager.Instance.UnregisterSmartObject(this);
        }

        public IEnumerable<string> GetCapabilities()
        {
            return _config.GetCapabilities();
        }

        public float GetCapabilityValue(string capabilityName)
        {
            return _config.GetCapability(name);
        }

        public abstract void OnCompleteWithFailure(SmartAgent m_Agent);

        public abstract void OnCompleteWithSuccess(SmartAgent agent);

        public SmartInteraction<SmartAgent> RequestInteraction(SmartAgent agent)
        {
            BehaviourTree bt = new BehaviourTree();
            var movement = bt.CreateLeafNode(GetMovementAction(agent));
            var action = bt.CreateLeafNode(GetRequestedAction(agent));
            var seq = bt.CreateComposite<SequencerNode>(false, movement, action);
            bt.SetRootNode(seq);
            return new SmartInteraction<SmartAgent>(this, new SubsystemAction(bt));
        }

        protected abstract Action GetRequestedAction(SmartAgent agent);

        public abstract bool ValidateAgent(SmartAgent agent);

        private Action GetMovementAction(SmartAgent agent)
        {
            IAgentMovement movementComponent = agent.gameObject.GetComponent<IAgentMovement>();
            var targetPosition = GetTargetPosition(agent);
            var action = new FunctionalAction(() => movementComponent.Move(targetPosition));
            return action;
        }

        protected abstract Vector3 GetTargetPosition(SmartAgent agent);
    }
}

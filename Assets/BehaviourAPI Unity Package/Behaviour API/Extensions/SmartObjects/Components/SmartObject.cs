using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.SmartObjects
{
    using BehaviourAPI.SmartObjects;
    using Core;
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
            Debug.Log(GetType().Name);
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

        public virtual void OnComplete(SmartAgent agent, Status status)
        {
            return;
        }

        public virtual void InitInteraction(SmartAgent agent)
        {
            return;
        }

        public virtual void ReleaseInteraction(SmartAgent agent)
        {
            return;
        }

        public SmartInteraction<SmartAgent> RequestInteraction(SmartAgent agent)
        {
            Action action = GetRequestedAction(agent);
            return new SmartInteraction<SmartAgent>(this, action);
        }

        public abstract bool ValidateAgent(SmartAgent agent);

        protected abstract Action GetRequestedAction(SmartAgent agent);
    }
}

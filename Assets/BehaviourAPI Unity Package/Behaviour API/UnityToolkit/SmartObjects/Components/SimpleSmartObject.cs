using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Smart object that provide always the same interaction
    /// </summary>
    public abstract class SimpleSmartObject : SmartObject
    {
        [SerializeField] SmartInteractionProvider interactionProvider;

        public Dictionary<string, float> GetCapabilities()
        {
            return interactionProvider.GetCapabilityMap();
        }

        public override float GetCapabilityValue(string capabilityName)
        {
            return interactionProvider.GetCapabilityMap().GetValueOrDefault(capabilityName);
        }

        public override SmartInteraction RequestInteraction(SmartAgent agent, RequestData requestData)
        {
            SmartInteraction interaction = interactionProvider.CreateInteraction(agent);
            SetInteractionEvents(interaction);
            return interaction;
        }

        public virtual void SetInteractionEvents(SmartInteraction interaction) 
        {
            return;
        }
    }
}

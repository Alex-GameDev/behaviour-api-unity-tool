using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    using SmartObjects;
    using Core.Actions;
    using System.Collections.Generic;

    public abstract class SmartInteractionProvider : MonoBehaviour
    {
        [SerializeField] CapabilityMap capabilities;

        public SmartInteraction CreateInteraction(SmartAgent smartAgent)
        {
            Action action = GetInteractionAction(smartAgent);
            SmartInteraction smartInteraction = new SmartInteraction(action, smartAgent,GetCapabilityMap());
            return smartInteraction;
        }

        public abstract Action GetInteractionAction(SmartAgent agent);

        public Dictionary<string, float> GetCapabilityMap() => capabilities;
    }
}

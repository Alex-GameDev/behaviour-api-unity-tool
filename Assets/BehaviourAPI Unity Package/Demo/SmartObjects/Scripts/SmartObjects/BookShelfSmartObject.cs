using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    using Core.Actions;
    using SmartObjects;
    using UnityToolkit;

    public class BookShelfSmartObject : DirectSmartObject
    {
        [SerializeField, Range(0f, 1f)] float leisureCapability = 0.3f;

        public override Dictionary<string, float> GetCapabilities()
        {
            Dictionary<string, float> capabilities = new Dictionary<string, float>();
            capabilities["leisure"] = leisureCapability;
            return capabilities;
        }

        public override float GetCapabilityValue(string capabilityName)
        {
            if (capabilityName == "leisure") return leisureCapability;
            else return 0f;
        }

        public override bool ValidateAgent(SmartAgent agent)
        {
            // Comprobar si hay asientos libres
            return true;
        }

        protected override Action GetUseAction(SmartAgent agent, RequestData requestData)
        {
            var seatAction = new SeatRequestAction(agent);
            return seatAction;
        }
    }
}
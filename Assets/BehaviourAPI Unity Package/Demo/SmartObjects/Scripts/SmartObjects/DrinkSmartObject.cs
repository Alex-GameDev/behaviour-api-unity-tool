using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    using BehaviourAPI.SmartObjects;
    using Core.Actions;
    using UnityToolkit.SmartObjects;
    public class DrinkSmartObject : SimpleSmartObject
    {
        [SerializeField] FridgeSmartObject _fridge;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return _fridge.ValidateAgent(agent);
        }

        protected override Action GenerateAction(SmartAgent agent, RequestData requestData)
        {
            return new DirectRequestAction(agent, _fridge);
        }
    }

}
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.SmartObjects;
using UnityEngine;
using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class ChickenSmartObject : SimpleSmartObject
    {
        [SerializeField] FridgeSmartObject _fridge;
        [SerializeField] OvenSmartObject _oven;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return _fridge.ValidateAgent(agent);
        }

        protected override Action GenerateAction(SmartAgent agent, RequestData requestData)
        {
            SequenceAction sequence = new SequenceAction();

            sequence.SubActions.Add(new DirectRequestAction(agent, _fridge));
            sequence.SubActions.Add(new DirectRequestAction(agent, _oven));
            sequence.SubActions.Add(new SeatRequestAction(agent));

            return sequence;
        }
    }

}
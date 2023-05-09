using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class ChickenSmartObject : SmartObject
    {
        [SerializeField] FridgeSmartObject _fridge;
        [SerializeField] OvenSmartObject _oven;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return _fridge.ValidateAgent(agent);
        }

        protected override Action GetRequestedAction(SmartAgent agent)
        {
            var bt = new BehaviourTree();
            bt.SetRootNode(
                bt.CreateComposite<SequencerNode>(false,
                    bt.CreateLeafNode(new DirectRequestAction(agent, _fridge)),
                    bt.CreateLeafNode(new DirectRequestAction(agent, _oven)),
                    bt.CreateLeafNode(new SeatRequestAction(agent))
            ));
            return new SubsystemAction(bt);
        }
    }

}
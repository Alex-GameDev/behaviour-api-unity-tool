using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class ChickenSmartObject : SmartObject
    {
        [SerializeField] FridgeSmartObject _fridge;
        [SerializeField] OvenSmartObject _oven;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return _fridge.ValidateAgent(agent);
        }

        protected override Action GetRequestedAction(SmartAgent agent, string interactionName = null)
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
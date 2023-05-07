using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class ChickenSmartObject : SmartObject
{
    [SerializeField] FridgeSmartObject _fridge;

    public override void OnCompleteWithFailure(SmartAgent m_Agent)
    {

    }

    public override void OnCompleteWithSuccess(SmartAgent agent)
    {

    }

    public override bool ValidateAgent(SmartAgent agent)
    {
        return _fridge.ValidateAgent(agent);
    }

    protected override Action GetRequestedAction(SmartAgent agent)
    {
        var bt = new BehaviourTree();
        bt.SetRootNode(
            bt.CreateComposite<SequencerNode>(false,
                bt.CreateLeafNode(_fridge.RequestInteraction(agent).Action),
                bt.CreateLeafNode(new UnityTypedRequestAction<OvenSmartObject>(agent)),
                bt.CreateLeafNode(new SeatRequestAction(agent))
        ));
        return new SubsystemAction(bt);
    }

    protected override Vector3 GetTargetPosition(SmartAgent agent)
    {
        return agent.transform.position;
    }
}

using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class AppleSmartObject : SmartObject
{
    //TODO: Crear clase FridgeItem que represente un smartObject que se usa cogiéndolo de la nevera y después usándolo.
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
        return _fridge.RequestInteraction(agent).Action;

    }

    protected override Vector3 GetTargetPosition(SmartAgent agent)
    {
        return agent.transform.position;
    }
}

using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class AppleSmartObject : SmartObject
{
    //TODO: Crear clase FridgeItem que represente un smartObject que se usa cogiéndolo de la nevera y después usándolo.
    [SerializeField] FridgeSmartObject _fridge;

    public override bool ValidateAgent(SmartAgent agent)
    {
        return true;
    }

    protected override Action GetRequestedAction(SmartAgent agent)
    {
        return _fridge.RequestInteraction(agent).Action;
    }
}

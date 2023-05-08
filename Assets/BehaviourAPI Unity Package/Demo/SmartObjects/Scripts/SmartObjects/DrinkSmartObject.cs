using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class DrinkSmartObject : SmartObject
{
    [SerializeField] FridgeSmartObject _fridge;

    public override bool ValidateAgent(SmartAgent agent)
    {
        return _fridge.ValidateAgent(agent);
    }

    protected override Action GetRequestedAction(SmartAgent agent)
    {
        return _fridge.RequestInteraction(agent).Action;

    }
}

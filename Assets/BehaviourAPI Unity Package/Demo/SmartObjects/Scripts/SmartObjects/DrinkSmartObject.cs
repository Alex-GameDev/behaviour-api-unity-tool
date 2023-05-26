using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

public class DrinkSmartObject : SmartObject
{
    [SerializeField] FridgeSmartObject _fridge;

    public override bool ValidateAgent(SmartAgent agent)
    {
        return _fridge.ValidateAgent(agent);
    }

    protected override Action GetRequestedAction(SmartAgent agent, string interactionName = null)
    {
        return new DirectRequestAction(agent, _fridge, interactionName);

    }
}

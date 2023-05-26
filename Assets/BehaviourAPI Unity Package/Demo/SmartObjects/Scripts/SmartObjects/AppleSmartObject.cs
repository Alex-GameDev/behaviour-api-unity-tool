using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

public class AppleSmartObject : SmartObject
{
    //TODO: Crear clase FridgeItem que represente un smartObject que se usa cogi�ndolo de la nevera y despu�s us�ndolo.
    [SerializeField] FridgeSmartObject _fridge;

    public override bool ValidateAgent(SmartAgent agent)
    {
        return true;
    }

    protected override Action GetRequestedAction(SmartAgent agent, string interactionName = null)
    {
        return new DirectRequestAction(agent, _fridge, interactionName);
    }
}

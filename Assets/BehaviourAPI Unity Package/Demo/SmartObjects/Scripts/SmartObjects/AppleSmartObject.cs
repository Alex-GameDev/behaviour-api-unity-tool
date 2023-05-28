using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;
using BehaviourAPI.SmartObjects;

public class AppleSmartObject : SimpleSmartObject
{
    //TODO: Crear clase FridgeItem que represente un smartObject que se usa cogi�ndolo de la nevera y despu�s us�ndolo.
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

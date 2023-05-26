using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityToolkit.Demos;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

public class ComputerDesktopSmartObject : SmartObject
{
    [SerializeField] Transform target;
    [SerializeField] SeatSmartObject seat;

    public override bool ValidateAgent(SmartAgent agent)
    {
        return seat.ValidateAgent(agent);
    }

    protected override Action GetRequestedAction(SmartAgent agent, string interactionName = null)
    {
        return new DirectRequestAction(agent, seat, interactionName);
    }
}

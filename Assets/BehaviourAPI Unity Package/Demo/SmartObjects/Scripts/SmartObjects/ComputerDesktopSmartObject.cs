using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Demos;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
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

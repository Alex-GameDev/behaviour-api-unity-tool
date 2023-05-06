using BehaviourAPI.Unity.Demos;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using System.Linq;
using UnityEngine;

public class SeatRequestAction : UnityRequestAction
{
    public SeatRequestAction()
    {
    }

    public SeatRequestAction(SmartAgent agent) : base(agent)
    {
    }

    protected override SmartObject GetSmartObject(SmartAgent agent)
    {
        var validSeats = SmartObjectManager.Instance.RegisteredObjects.FindAll(obj => obj is SeatSmartObject && obj.ValidateAgent(agent));
        Debug.Log("Valid seats: " + validSeats.Count);

        var closestValidSeat = validSeats.OrderBy(obj => Vector3.Distance(obj.transform.position, agent.gameObject.transform.position)).FirstOrDefault();
        return closestValidSeat;
    }
}

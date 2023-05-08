using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;

public class SeatRequestAction : UnityRequestAction
{
    public bool closest = false;

    public float maxDistance = -1f;


    public SeatRequestAction()
    {
    }

    public SeatRequestAction(SmartAgent agent, bool closest = false, float maxDistance = -1) : base(agent)
    {
        this.closest = closest;
        this.maxDistance = maxDistance;
    }

    protected override SmartObject GetSmartObject(SmartAgent agent)
    {
        if (SeatManager.Instance != null)
        {
            if (closest)
            {
                return SeatManager.Instance.GetClosestSeat(agent.transform.position);
            }
            else if (maxDistance > 0)
            {
                return SeatManager.Instance.GetRandomSeat(agent.transform.position, maxDistance);
            }
            else
            {
                return SeatManager.Instance.GetRandomSeat();
            }
        }
        return null;
    }
}

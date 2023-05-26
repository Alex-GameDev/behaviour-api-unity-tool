using BehaviourAPI.UnityToolkit.Demos;
using BehaviourAPI.UnityToolkit.SmartObjects;
using BehaviourAPI.UnityToolkit;

public class SeatRequestAction : UnityRequestAction
{
    public bool closest = false;

    public float useTime = 5f;

    public SeatRequestAction()
    {
    }

    public SeatRequestAction(SmartAgent agent, bool closest = false, float useTime = 5f) : base(agent)
    {
        this.closest = closest;
        this.useTime = useTime;
    }

    protected override SmartObject GetSmartObject(SmartAgent agent)
    {
        SeatSmartObject requestedSeat;
        if (SeatManager.Instance != null)
        {
            if (closest)
            {
                requestedSeat = SeatManager.Instance.GetClosestSeat(agent.transform.position);
            }
            else
            {
                requestedSeat = SeatManager.Instance.GetRandomSeat();
            }
        }
        else
        {
            requestedSeat = null;
        }

        if (requestedSeat != null)
            requestedSeat.UseTime = useTime;

        return requestedSeat;
    }
}

using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Unity.SmartObjects;
    using BehaviourAPI.UnityExtensions;

    public class TVSmartObject : SmartObject
    {
        [SerializeField] float maxDistance;

        [SerializeField] float useTime;
        public override bool ValidateAgent(SmartAgent agent)
        {
            return true;
        }

        protected override Action GetRequestedAction(SmartAgent agent)
        {
            var seatRequestAction = new TVSeatRequestAction(agent, transform, maxDistance, useTime);
            return seatRequestAction;
        }

        private class TVSeatRequestAction : UnityRequestAction
        {
            public Transform requestTf;
            public float maxDistance;
            public float useTime = 5f;

            public TVSeatRequestAction(SmartAgent agent, Transform requestTf, float maxDistance, float useTime) : base(agent)
            {
                this.requestTf = requestTf;
                this.maxDistance = maxDistance;
                this.useTime = useTime;
            }


            protected override SmartObject GetSmartObject(SmartAgent agent)
            {
                SeatSmartObject requestedSeat;
                if (SeatManager.Instance != null)
                {
                    requestedSeat = SeatManager.Instance.GetRandomSeat(requestTf.position, maxDistance);
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
    }

}
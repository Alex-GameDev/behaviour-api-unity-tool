using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.SmartObjects;
    using BehaviourAPI.UnityToolkit;

    public class TVSmartObject : SimpleSmartObject
    {
        [SerializeField] float maxDistance;

        [SerializeField] float useTime;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return true;
        }

        protected override Action GenerateAction(SmartAgent agent, RequestData requestData)
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

            protected override RequestData GetRequestData()
            {
                return new SeatRequestData(useTime);
            }

            //protected override SmartObject GetSmartObject(SmartAgent agent)
            //{
            //    SeatSmartObject requestedSeat;
            //    if (SeatManager.Instance != null)
            //    {
            //        requestedSeat = SeatManager.Instance.GetRandomSeat(requestTf.position, maxDistance);
            //    }
            //    else
            //    {
            //        requestedSeat = null;
            //    }

            //    if (requestedSeat != null)
            //        requestedSeat.UseTime = useTime;

            //    return requestedSeat;
            //}

            protected override ISmartObjectProvider<SmartAgent> GetSmartObjectProvider()
            {
                return null;
            }
        }
    }

}
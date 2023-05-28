using BehaviourAPI.Core.Actions;
using UnityEngine;
using BehaviourAPI.SmartObjects;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class ComputerDesktopSmartObject : SimpleSmartObject
    {
        [SerializeField] Transform target;
        [SerializeField] SeatSmartObject seat;

        public override bool ValidateAgent(SmartAgent agent)
        {
            return seat.ValidateAgent(agent);
        }

        protected override Action GenerateAction(SmartAgent agent, RequestData requestData)
        {
            return new DirectRequestAction(agent, seat);
        }
    }

}
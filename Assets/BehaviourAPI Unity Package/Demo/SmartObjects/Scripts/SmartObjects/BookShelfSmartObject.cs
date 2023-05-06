namespace BehaviourAPI.Unity.Demos
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.UnityExtensions;
    using Unity.SmartObjects;
    using UnityEngine;

    public class BookShelfSmartObject : SmartObject
    {
        [SerializeField] Transform _targetTransform;

        public override void OnCompleteWithFailure(SmartAgent m_Agent)
        {
        }

        public override void OnCompleteWithSuccess(SmartAgent agent)
        {
        }

        public override bool ValidateAgent(SmartAgent agent)
        {
            // Comprobar si hay asientos libres
            return true;
        }

        protected override Action GetRequestedAction(SmartAgent agent)
        {
            var seatAction = new SeatRequestAction(agent);
            return seatAction;
        }

        protected override Vector3 GetTargetPosition(SmartAgent agent)
        {
            return _targetTransform.position;
        }
    }
}
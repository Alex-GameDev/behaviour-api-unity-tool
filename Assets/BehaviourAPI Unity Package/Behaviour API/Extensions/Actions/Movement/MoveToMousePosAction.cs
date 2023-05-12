using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;
    /// <summary>
    /// Custom action that moves an agent to a given position, returning success when the position is arrived.
    /// </summary>

    [SelectionGroup("MOVEMENT")]
    public class MoveToMousePosAction : UnityAction
    {
        /// <summary>
        /// Create a new MoveToMousePosAction
        /// </summary>
        public MoveToMousePosAction()
        {
        }

        public override void Start()
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit, 100f))
            {
                context.Movement.SetTarget(hit.point);
            }
        }

        public override void Stop()
        {
            context.Movement.CancelMove();
        }

        public override Status Update()
        {
            if (context.Movement.HasArrived())
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }

        }

        public override string DisplayInfo => "Move to mouse position";
    }
}


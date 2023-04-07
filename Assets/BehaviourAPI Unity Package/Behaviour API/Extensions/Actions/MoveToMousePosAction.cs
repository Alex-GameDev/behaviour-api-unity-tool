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
        public float speed;

        public MoveToMousePosAction()
        {
        }

        public MoveToMousePosAction(float speed)
        {
            this.speed = speed;
        }

        public override void Start()
        {
            context.NavMeshAgent.speed = speed;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit, 100f))
            {
                context.NavMeshAgent.destination = new Vector3(hit.point.x, context.NavMeshAgent.transform.position.y, hit.point.z);
            }
        }

        public override void Stop()
        {
            context.NavMeshAgent.speed = 0f;
        }

        public override Status Update()
        {
            if (!context.NavMeshAgent.hasPath || context.NavMeshAgent.speed == -1)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }

        }

        public override string DisplayInfo => "Move to mousePosition";
    }
}


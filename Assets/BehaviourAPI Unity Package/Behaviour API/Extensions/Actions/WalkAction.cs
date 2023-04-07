using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    [SelectionGroup("MOVEMENT")]
    public class WalkAction : UnityAction
    {
        public float Speed;
        public Vector3 Target;

        public WalkAction()
        {
        }

        public WalkAction(Vector3 target, float speed)
        {
            Speed = speed;
            Target = target;
        }

        public override void Start()
        {
            context.NavMeshAgent.destination = Target;
            context.NavMeshAgent.speed = Speed;
        }

        public override void Stop()
        {
        }

        public override Status Update()
        {
            if (Vector3.Distance(context.NavMeshAgent.transform.position, Target) < .5f)
            {
                return Status.Success;
            }
            else
                return Status.Running;
        }

        public override string DisplayInfo => "Walk to $Target";
    }
}


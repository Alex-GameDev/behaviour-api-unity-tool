using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that moves the agent to a determined position.
    /// </summary>
    [SelectionGroup("MOVEMENT")]
    public class WalkAction : UnityAction
    {
        /// <summary>
        /// The movement speed of the agent.
        /// </summary>
        public float Speed;

        /// <summary>
        /// The target position.
        /// </summary>
        public Vector3 Target;

        /// <summary>
        /// Create a new WalkAction
        /// </summary>
        public WalkAction()
        {
        }

        /// <summary>
        /// Create a new WalkAction
        /// </summary>
        /// <param name="target">The target position.</param>
        /// <param name="speed">The movement speed of the agent. </param>
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


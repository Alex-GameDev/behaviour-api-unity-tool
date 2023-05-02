using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that moves an agent to a random position.
    /// </summary>
    [SelectionGroup("MOVEMENT")]
    public class PatrolAction : UnityAction
    {
        /// <summary>
        /// The movement speed of the agent.
        /// </summary>
        public float speed;

        /// <summary>
        /// The max distance of the target point.
        /// </summary>
        public float maxDistance;

        Vector3 _target;

        /// <summary>
        /// Create a new PatrolAction
        /// </summary>
        public PatrolAction()
        {
        }

        /// <summary>
        /// Create a new PatrolAction
        /// </summary>
        /// <param name="speed">The movement speed of the agent.</param>
        /// <param name="maxDistance">The max distance of the target point.</param>
        public PatrolAction(float speed, float maxDistance)
        {
            this.speed = speed;
            this.maxDistance = maxDistance;
        }

        public override void Start()
        {
            context.NavMeshAgent.speed = speed;
            Vector3 positionToRun = Random.insideUnitSphere * maxDistance;
            _target = new Vector3(positionToRun.x, context.NavMeshAgent.transform.position.y, positionToRun.z);
            context.NavMeshAgent.destination = _target;

        }

        public override void Stop()
        {
            context.NavMeshAgent.speed = 0f;
        }

        public override Status Update()
        {
            if(context.NavMeshAgent.destination != _target) context.NavMeshAgent.destination = _target;
            if (!context.NavMeshAgent.hasPath || context.NavMeshAgent.velocity.sqrMagnitude == -1f ||
                Vector3.Distance(context.NavMeshAgent.transform.position, _target) < .1f)
            {
                return Status.Success;
            }
            else
                return Status.Running;
        }

        public override string DisplayInfo => "Move to a random position in a $maxDistance radius circle.";
    }
}

using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that makes the agent chase another object.
    /// </summary>
    [SelectionGroup("MOVEMENT")]
    public class ChaseAction : UnityAction
    {
        /// <summary>
        /// The movement speed of the agent.
        /// </summary>
        public float speed;

        /// <summary>
        /// The transform that the agent chase.
        /// </summary>
        public Transform target;

        /// <summary>
        /// The distance that the agent must be to its target to end with success.
        /// </summary>
        public float maxDistance;

        /// <summary>
        /// The max time that the agent will chase.
        /// </summary>
        public float maxTime;

        float _currentTime;

        /// <summary>
        /// Create a new Chase Action.
        /// </summary>
        public ChaseAction() { }

        /// <summary>
        /// Create a new Chase Action.
        /// </summary>
        /// <param name="target">The transform that the agent chase.</param>
        /// <param name="speed">The movement speed of the agent.</param>
        /// <param name="maxDistance">The distance that the agent must be to its target to end with success.</param>
        /// <param name="maxTime">The max time that the agent will chase.</param>
        public ChaseAction(Transform target, float speed, float maxDistance, float maxTime)
        {
            this.speed = speed;
            this.target = target;
            this.maxTime = maxTime;
            this.maxDistance = maxDistance;
        }

        public override void Start()
        {
            context.NavMeshAgent.speed = speed;
            _currentTime = 0f;
            context.NavMeshAgent.destination = new Vector3(target.transform.position.x, context.NavMeshAgent.transform.position.y, target.transform.position.z);
        }

        public override void Stop()
        {
            context.NavMeshAgent.speed = 0f;
        }

        public override Status Update()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime > maxTime)
            {
                return Status.Success;
            }
            else
            {
                float distance = Vector3.Distance(context.NavMeshAgent.transform.position, target.position);

                if (distance < .3f) return Status.Success;
                else if (distance > maxDistance) return Status.Failure;
            }
            return Status.Running;
        }

        public override string DisplayInfo => "Chase $target for $maxTime seconds";
    }

}
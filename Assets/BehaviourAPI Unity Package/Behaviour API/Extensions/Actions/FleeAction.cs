using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that moves an agent away from a transform, returning success when the position is arrived.
    /// </summary>

    [SelectionGroup("MOVEMENT")]
    public class FleeAction : UnityAction
    {
        /// <summary>
        /// The transformation from which the agent flees.
        /// </summary>
        public Transform OtherTransform;

        /// <summary>
        /// The movement speed of the agent.
        /// </summary>
        public float speed;

        /// <summary>
        /// The distance of the target point.
        /// </summary>
        public float distance;

        /// <summary>
        /// The maximum time the agent will run.
        /// </summary>
        public float maxTimeRunning;

        float _timeRunning;

        Vector3 _target;

        /// <summary>
        /// Create a new flee action.
        /// </summary>
        public FleeAction() { }

        /// <summary>
        /// Create a new flee action
        /// </summary>
        /// <param name="otherTransform">The transformation from which the agent flees.</param>
        /// <param name="speed">The movement speed of the agent.</param>
        /// <param name="distance">The distance of the target point.</param>
        /// <param name="maxTimeRunning">The maximum time the agent will run.</param>
        public FleeAction(Transform otherTransform, float speed, float distance, float maxTimeRunning)
        {
            this.speed = speed;
            this.distance = distance;
            this.maxTimeRunning = maxTimeRunning;
        }

        public override void Start()
        {
            _timeRunning = 0f;
            context.NavMeshAgent.speed = speed;
            Vector3 positionToRun = Random.insideUnitSphere * distance;
            _target = new Vector3(positionToRun.x, context.NavMeshAgent.transform.position.y, positionToRun.z);
            context.NavMeshAgent.destination = _target;
        }

        public override void Stop()
        {
            context.NavMeshAgent.speed = 0f;
        }

        public override Status Update()
        {
            _timeRunning += Time.deltaTime;

            if(context.NavMeshAgent.destination != _target) context.NavMeshAgent.destination = _target;

            if (_timeRunning > maxTimeRunning)
            {
                return Status.Failure;
            }
            else if (!context.NavMeshAgent.hasPath || context.NavMeshAgent.velocity.sqrMagnitude == 0f)
            {
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
        public override string DisplayInfo => "Flee to random direction";

    }
}
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;

    /// <summary>
    /// Action that moves the agent along a path.
    /// </summary>
    [SelectionGroup("MOVEMENT")]
    public class PathingAction : UnityAction
    {
        /// <summary>
        /// The points that forms the path.
        /// </summary>
        public List<Vector3> positions;

        /// <summary>
        /// The movement speed of the agent.
        /// </summary>
        public float speed;

        /// <summary>
        /// The distance the agent must be from one point to go to the next.
        /// </summary>
        public float distanceThreshold;

        int currentTargetPosId;

        /// <summary>
        /// Create a new PathingAction.
        /// </summary>
        public PathingAction() { }

        /// <summary>
        /// Create a new PathingAction.
        /// </summary>
        /// <param name="positions">The points that forms the path.</param>
        /// <param name="speed">The movement speed of the agent.</param>
        /// <param name="distanceThreshold">The distance the agent must be from one point to go to the next.</param>
        public PathingAction(List<Vector3> positions, float speed, float distanceThreshold)
        {
            this.positions = positions;
            this.speed = speed;
            this.distanceThreshold = distanceThreshold;
        }

        public override string DisplayInfo => "Move between positions.";

        public override void Start()
        {
            currentTargetPosId = 0;

            if(positions.Count > 0)
                context.Transform.forward = (positions[currentTargetPosId] - context.Transform.position).normalized;
        }

        public override Status Update()
        {
            if (positions.Count == 0) return Status.Failure;

            if (Vector3.Distance(context.Transform.position, positions[currentTargetPosId]) < distanceThreshold)
            {
                currentTargetPosId++;

                if (currentTargetPosId >= positions.Count)
                {
                    currentTargetPosId = 0;
                    return Status.Success;
                }
                else
                {
                    context.Transform.forward = (positions[currentTargetPosId] - positions[currentTargetPosId - 1]).normalized;
                }
            }

            var currentPos = context.Transform.position;
            var rawMovement = positions[currentTargetPosId] - currentPos;
            var maxDistance = rawMovement.magnitude;
            var movement = rawMovement.normalized * speed * Time.deltaTime;
            context.Transform.position = Vector3.MoveTowards(currentPos, currentPos + movement, maxDistance);
            return Status.Running;
        }
    }
}

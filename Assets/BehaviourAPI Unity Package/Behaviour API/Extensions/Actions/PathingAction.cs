using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;
    [SelectionGroup("MOVEMENT")]
    public class PathingAction : UnityAction
    {
        public List<Vector3> positions;
        public float speed;
        public float distanceThreshold;

        int currentTargetPosId;

        public PathingAction() { }

        public PathingAction(List<Vector3> positions, float speed, float distanceThreshold)
        {
            this.positions = positions;
            this.speed = speed;
            this.distanceThreshold = distanceThreshold;
        }

        public override string DisplayInfo => "Move between $positions at $speed";

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

        public override void Stop()
        {
        }
    }
}

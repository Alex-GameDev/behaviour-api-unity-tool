using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class PathingAction : UnityAction
    {
        public Transform transform;

        public List<Vector3> positions;
        public float speed;
        public float distanceThreshold;

        int currentTargetPosId;

        public PathingAction() { }

        public PathingAction(Transform transform, List<Vector3> positions, float speed, float distanceThreshold)
        {
            this.transform = transform;
            this.positions = positions;
            this.speed = speed;
            this.distanceThreshold = distanceThreshold;
        }

        public override string DisplayInfo => "Move between $positions at $speed";

        protected override void OnStart()
        {
            currentTargetPosId = 0;
        }

        protected override void OnUpdate()
        {
            if (positions.Count == 0) return;

            if (Vector3.Distance(transform.position, positions[currentTargetPosId]) < distanceThreshold)
            {
                currentTargetPosId++;
                if (currentTargetPosId >= positions.Count)
                {
                    Success();
                    currentTargetPosId = 0;
                }
            }

            var currentPos = transform.position;
            var rawMovement = positions[currentTargetPosId] - currentPos;
            var maxDistance = rawMovement.magnitude;
            var movement = rawMovement.normalized * speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(currentPos, currentPos + movement, maxDistance);
        }
    }
}

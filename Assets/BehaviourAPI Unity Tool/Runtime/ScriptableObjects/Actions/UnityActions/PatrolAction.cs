using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Runtime
{
    public class PatrolAction : UnityAction
    {
        public Transform transform;

        public List<Vector3> positions;
        public float speed;
        public float distanceThreshold;

        int currentTargetPosId;

        protected override void Start()
        {
            currentTargetPosId = 0;
        }

        protected override void Update()
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

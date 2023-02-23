using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    /// <summary>
    /// Custom action that moves an agent away from a transform, returning success when the position is arrived.
    /// </summary>
    public class FleeAction : UnityAction
    {
        public float speed;
        public float distance;
        public float maxTimeRunning;
        float _timeRunning;

        Vector3 _target;

        public FleeAction() { }

        public FleeAction(float speed, float distance, float maxTimeRunning)
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

        public override string DisplayInfo => "Flee to random direction";

        public override Status Update()
        {
            _timeRunning += Time.deltaTime;

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
    }
}
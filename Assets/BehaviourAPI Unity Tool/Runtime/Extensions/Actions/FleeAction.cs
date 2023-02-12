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
        public NavMeshAgent agent;
        public float speed;
        public float distance;
        public float maxTimeRunning;
        float _timeRunning;

        Vector3 _target;

        public FleeAction() { }

        public FleeAction(NavMeshAgent agent, float speed, float distance, float maxTimeRunning)
        {
            this.agent = agent;
            this.speed = speed;
            this.distance = distance;
            this.maxTimeRunning = maxTimeRunning;
        }

        public override void Start()
        {
            _timeRunning = 0f;
            agent.speed = speed;
            Vector3 positionToRun = Random.insideUnitSphere * distance;
            _target = new Vector3(positionToRun.x, agent.transform.position.y, positionToRun.z);
            agent.destination = _target;
        }

        public override void Stop()
        {
            agent.speed = 0f;
        }

        public override string DisplayInfo => "Flee to random direction";

        public override Status Update()
        {
            _timeRunning += Time.deltaTime;

            if (_timeRunning > maxTimeRunning)
            {
                return Status.Failure;
            }
            else if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
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
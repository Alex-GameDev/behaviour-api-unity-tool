using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    /// <summary>
    /// Custom action that moves an agent to a given position, returning success when the position is arrived.
    /// </summary>
    public class PatrolAction : UnityAction
    {
        public NavMeshAgent agent;
        public float speed;
        public float maxDistance;
        Vector3 _target;

        public PatrolAction()
        {
        }

        public PatrolAction(NavMeshAgent agent, float speed, float maxDistance)
        {
            this.agent = agent;
            this.speed = speed;
            this.maxDistance = maxDistance;
        }

        public override void Start()
        {
            agent.speed = speed;
            Vector3 positionToRun = Random.insideUnitSphere * maxDistance;
            _target = new Vector3(positionToRun.x, agent.transform.position.y, positionToRun.z);
            agent.destination = _target;

        }

        public override void Stop()
        {
            agent.speed = 0f;
        }

        public override Status Update()
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == -1f ||
                Vector3.Distance(agent.transform.position, _target) < .1f)
            {
                return Status.Success;
            }
            else
                return Status.Running;
        }

        public override string DisplayInfo => "Move randomly";
    }
}

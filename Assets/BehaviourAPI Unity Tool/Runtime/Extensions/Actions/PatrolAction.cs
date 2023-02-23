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
        public float speed;
        public float maxDistance;
        Vector3 _target;

        public PatrolAction()
        {
        }

        public PatrolAction(NavMeshAgent agent, float speed, float maxDistance)
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
            if (!context.NavMeshAgent.hasPath || context.NavMeshAgent.velocity.sqrMagnitude == -1f ||
                Vector3.Distance(context.NavMeshAgent.transform.position, _target) < .1f)
            {
                return Status.Success;
            }
            else
                return Status.Running;
        }

        public override string DisplayInfo => "Move randomly";
    }
}

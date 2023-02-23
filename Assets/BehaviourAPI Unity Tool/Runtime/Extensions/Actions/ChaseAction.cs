using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    public class ChaseAction : UnityAction
    {
        public float speed;
        public Transform target;
        public float maxDistance;
        public float maxTime;

        NavMeshAgent _agent;

        float _currentTime;

        public ChaseAction() { }

        public ChaseAction(NavMeshAgent agent, Transform target, float speed, float maxDistance, float maxTime)
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

            // Si se ha acabado el tiempo
            if (_currentTime > maxTime)
            {
                return Status.Success;
            }
            else
            {
                float distance = Vector3.Distance(context.NavMeshAgent.transform.position, target.position);
                // Si ha alcanzado el objetivo
                if (distance < .3f) return Status.Success;

                // Si el objetivo ha escapado
                else if (distance > maxDistance) return Status.Failure;
            }
            return Status.Running;
        }

        public override string DisplayInfo => "Chase $target for $maxTime seconds";
    }

}
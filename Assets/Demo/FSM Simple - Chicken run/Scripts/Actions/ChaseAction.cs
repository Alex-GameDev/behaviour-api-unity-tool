using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;
using UnityEngine.AI;

public class ChaseAction : Action
{
    public NavMeshAgent Agent;
    public float Speed;
    public Transform Target;
    public float MaxDistance;
    public float MaxTime;

    float _currentTime;

    public ChaseAction(NavMeshAgent agent, Transform target, float speed, float maxDistance, float maxTime)
    {
        Agent = agent;
        Speed = speed;
        Target = target;
        MaxTime = maxTime;
        MaxDistance = maxDistance;
    }

    public override void Start()
    {
        Agent.speed = Speed;
        _currentTime = 0f;
        Agent.destination = new Vector3(Target.transform.position.x, Agent.transform.position.y, Target.transform.position.z);
    }

    public override void Stop()
    {
        Agent.speed = 0f;
    }

    public override Status Update()
    {
        _currentTime += Time.deltaTime;

        // Si se ha acabado el tiempo
        if (_currentTime > MaxTime) return Status.Failure;

        float distance = Vector3.Distance(Agent.transform.position, Target.position);

        // Si ha alcanzado el objetivo
        if (distance < .3f) return Status.Success;

        // Si el objetivo ha escapado
        else if (distance > MaxDistance) return Status.Failure;

        // Si continua persiguiendo
        else return Status.Running;
    }
}

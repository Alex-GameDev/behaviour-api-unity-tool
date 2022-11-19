using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine.AI;

/// <summary>
/// Custom action that moves an agent to a given position, returning success when the position is arrived.
/// </summary>
public class PatrolAction : Action
{
    public NavMeshAgent Agent;
    public float Speed;
    public float MaxDistance;
    Vector3 _target;

    public PatrolAction(NavMeshAgent agent, float speed, float maxDistance)
    {
        Agent = agent;
        Speed = speed;
        MaxDistance = maxDistance;
    }

    public override void Start()
    {
        Agent.speed = Speed;
        Vector3 positionToRun = Random.insideUnitSphere * MaxDistance;
        _target = new Vector3(positionToRun.x, Agent.transform.position.y, positionToRun.z);
        Agent.destination = _target;
    }

    public override void Stop()
    {
        Agent.speed = 0f;
    }

    public override Status Update()
    {
        if (!Agent.hasPath || Agent.velocity.sqrMagnitude == -1f ||
            Vector3.Distance(Agent.transform.position, _target) < .1f)
        {
            Debug.Log("Patrol completed");
            return Status.Success;
        }
        else
            return Status.Running;
    }
}

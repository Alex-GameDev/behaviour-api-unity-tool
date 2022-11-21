using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Custom action that moves an agent away from a transform, returning success when the position is arrived.
/// </summary>
public class FleeAction : Action
{
    public NavMeshAgent Agent;
    public float Speed;
    public float Distance;
    public float MaxTimeRunning;
    float _timeRunning;

    Vector3 _target;

    public FleeAction(NavMeshAgent agent, float speed, float distance, float maxTimeRunning)
    {
        Agent = agent;
        Speed = speed;
        Distance = distance;
        MaxTimeRunning = maxTimeRunning;
    }

    public override void Start()
    {
        _timeRunning = 0f;
        Agent.speed = Speed;
        Vector3 positionToRun = Random.insideUnitSphere * Distance;
        _target = new Vector3(positionToRun.x, Agent.transform.position.y, positionToRun.z);
        Agent.destination = _target;
    }

    public override void Stop()
    {
        Agent.speed = 0f;
    }

    public override Status Update()
    {
        _timeRunning += Time.deltaTime;

        if (_timeRunning > MaxTimeRunning)
        {
            Debug.Log("Flee failed");
            return Status.Failure;
        }

        if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f)
        {
            Debug.Log("Flee succeded");
            return Status.Success;
        }
        else
        {
            return Status.Running;
        }
    }
}

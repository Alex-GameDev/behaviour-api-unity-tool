using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime.Extensions;
using UnityEngine;
using UnityEngine.AI;

public class WalkAction : UnityAction
{
    public NavMeshAgent Agent;
    public float Speed;
    public Vector3 Target;

    float _currentTime;

    public WalkAction()
    {
    }

    public WalkAction(NavMeshAgent agent, Vector3 target, float speed)
    {
        Agent = agent;
        Speed = speed;
        Target = target;
    }

    public override void Start()
    {
        Agent.destination = Target;
    }

    public override void Stop()
    {
        Agent.speed = 0f;
    }

    public override Status Update()
    {
        if (Vector3.Distance(Agent.transform.position, Target) < .5f)
        {
            return Status.Success;
        }
        else
            return Status.Running;
    }

    public override string DisplayInfo => "Walk to $target";
}

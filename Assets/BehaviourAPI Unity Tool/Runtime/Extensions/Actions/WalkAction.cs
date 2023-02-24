using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using UnityEngine;
using UnityEngine.AI;

[SelectionGroup("MOVEMENT")]
public class WalkAction : UnityAction
{
    public float Speed;
    public Vector3 Target;

    float _currentTime;

    public WalkAction()
    {
    }

    public WalkAction(Vector3 target, float speed)
    {
        Speed = speed;
        Target = target;
    }

    public override void Start()
    {
        if (context == null) Debug.Log("No context");
        if (context.NavMeshAgent == null) Debug.Log("No agent");
        context.NavMeshAgent.destination = Target;
    }

    public override void Stop()
    {
    }

    public override Status Update()
    {
        if (Vector3.Distance(context.NavMeshAgent.transform.position, Target) < .5f)
        {
            return Status.Success;
        }
        else
            return Status.Running;
    }

    public override string DisplayInfo => "Walk to $target";
}

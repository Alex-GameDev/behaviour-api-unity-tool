using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine.AI;

/// <summary>
/// Custom action that moves an agent to a given position, returning success when the position is arrived.
/// </summary>
public class MoveToMousePosAction : Action
{
    public NavMeshAgent Agent;
    public float Speed;

    public MoveToMousePosAction(NavMeshAgent agent, float speed)
    {
        Agent = agent;
        Speed = speed;
    }

    public override void Start()
    {
        Debug.Log("Start moving");
        Agent.speed = Speed;
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, 100f))
        {
            Agent.destination = new Vector3(hit.point.x, Agent.transform.position.y, hit.point.z);
        }
    }

    public override void Stop()
    {
        Agent.speed = 0f;
    }

    public override Status Update()
    {
        if (!Agent.hasPath || Agent.velocity.sqrMagnitude == 0f) return Status.Success;
        else return Status.Running;
    }
}

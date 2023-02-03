using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
    /// <summary>
    /// Custom action that moves an agent to a given position, returning success when the position is arrived.
    /// </summary>
    public class MoveToMousePosAction : UnityAction
    {
        public NavMeshAgent agent;
        public float speed;

        public MoveToMousePosAction()
        {
        }

        public MoveToMousePosAction(NavMeshAgent agent, float speed)
        {
            this.agent = agent;
            this.speed = speed;
        }

        public override void Start()
        {
            Debug.Log("Start moving");
            agent.speed = speed;
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out RaycastHit hit, 100f))
            {
                agent.destination = new Vector3(hit.point.x, agent.transform.position.y, hit.point.z);
            }
        }

        public override void Stop()
        {
            agent.speed = 0f;
        }

        protected override void OnUpdate()
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) Success();
        }

        public override string DisplayInfo => "Move $agent to mousePosition";
    }
}


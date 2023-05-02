using BehaviourAPI.Core;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.UnityExtensions
{
    public class NavmeshAgentMovement : MonoBehaviour, IAgentMovement
    {
        [SerializeField] float speed;

        NavMeshAgent m_NavMeshAgent;

        void Awake()
        {
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
        }

        public Status Move(Vector3 targetPos)
        {
            m_NavMeshAgent.destination = targetPos;
            m_NavMeshAgent.speed = speed;
            if (Vector3.Distance(transform.position, targetPos) < 1.5f)
            {
                m_NavMeshAgent.speed = 0f;
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }

        public Status Move(Vector3 targetPos, Quaternion targetRot)
        {
            m_NavMeshAgent.destination = targetPos;
            m_NavMeshAgent.speed = speed;
            if (Vector3.Distance(transform.position, targetPos) < 1.5f)
            {
                m_NavMeshAgent.path = null;
                m_NavMeshAgent.speed = 0f;
                return Status.Success;
            }
            else
            {
                return Status.Running;
            }
        }
    }
}

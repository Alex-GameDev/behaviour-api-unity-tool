using BehaviourAPI.Unity.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Demos
{
    public class ChickenFSMEditorRunner : EditorBehaviourRunner
    {
        [SerializeField] Transform _target;
        [SerializeField] Collider _visionCollider;
        NavMeshAgent _agent;

        protected override void OnAwake()
        {
            _agent = GetComponent<NavMeshAgent>();
            base.OnAwake();
        }

        public bool CheckWatchTarget()
        {
            if (_visionCollider.bounds.Contains(_target.position))
            {
                Vector3 direction = (_target.position - transform.position).normalized;
                Ray ray = new Ray(transform.position + transform.up, direction * 20);

                bool watchPlayer = Physics.Raycast(ray, out RaycastHit hit, 20) && hit.collider.gameObject.transform == _target;

                return watchPlayer;
            }
            return false;
        }
    }

}
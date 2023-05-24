using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class PlayerFSMEditorRunner : EditorBehaviourRunner
    {
        [SerializeField] private float minDistanceToChicken = 5;
        [SerializeField] private Transform chicken;
        [SerializeField] private Transform origin;

        private PushPerception _click;

        protected override void Init()
        {
            base.Init();
            _click = FindPushPerception("click");
        }

        // Update is called once per frame
        protected override void OnUpdated()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _click.Fire();
            }
            base.OnUpdated();
        }

        public bool CheckDistanceToChicken()
        {
            return Vector3.Distance(transform.position, chicken.transform.position) < minDistanceToChicken;
        }

        public void Restart()
        {
            transform.position = origin.position;
        }
    }

}
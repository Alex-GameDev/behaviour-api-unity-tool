using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    using Core;
    public class DelayAction : UnityAction
    {
        public float delayTime;

        float _currentTime;

        public DelayAction()
        {
        }

        public DelayAction(float delayTime)
        {
            this.delayTime = delayTime;
        }

        public override string DisplayInfo => "Wait $DelayTime seconds";

        public override void Start()
        {
            _currentTime = 0;
        }

        public override void Stop()
        {
            _currentTime = 0;
        }

        public override Status Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > delayTime)
            {
                return Status.Success;
            }
            else
                return Status.Running;
        }
    }
}

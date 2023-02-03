using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.Extensions
{
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
        protected override void OnStart()
        {
            _currentTime = 0;
        }

        protected override void OnStop()
        {
            _currentTime = 0;
        }

        protected override void OnUpdate()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime > delayTime)
            {
                Success();
            }
        }
    }
}

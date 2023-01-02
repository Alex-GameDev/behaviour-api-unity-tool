using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class DelayAction : UnityAction
    {
        public float DelayTime;

        float _currentTime;

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
            if(_currentTime > DelayTime)
            {
                Success();
            }
        }
    }
}

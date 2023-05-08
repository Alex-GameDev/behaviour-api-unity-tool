using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class OvenSmartObject : DirectSmartObject
    {
        [SerializeField] float useTime = 3f;
        [SerializeField] Light _light;

        float lieTime;

        private void Awake()
        {
            _light.enabled = false;
        }

        protected override Action GetUseAction(SmartAgent agent)
        {
            return new FunctionalAction(StartUsing, () => OnUpdate(agent), StopUsing);
        }

        void StartUsing()
        {
            lieTime = Time.time;
            _light.enabled = true;
        }

        void StopUsing()
        {
            if (_light != null)
                _light.enabled = false;
        }

        Status OnUpdate(SmartAgent smartAgent)
        {
            if (Time.time > lieTime + useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }

}
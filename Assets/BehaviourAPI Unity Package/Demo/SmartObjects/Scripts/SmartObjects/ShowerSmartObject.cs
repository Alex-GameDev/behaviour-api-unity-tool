using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class ShowerSmartObject : SmartObject
    {
        [SerializeField] Transform _targetTransform;
        [SerializeField] Transform _useTransform;
        [SerializeField] ParticleSystem _particleSystem;

        [SerializeField] float useTime = 5f;

        SmartAgent _owner;

        float lieTime;

        public override void OnCompleteWithFailure(SmartAgent m_Agent)
        {
        }

        public override void OnCompleteWithSuccess(SmartAgent agent)
        {
        }

        public override bool ValidateAgent(SmartAgent agent)
        {
            return _owner == null;
        }

        protected override Action GetRequestedAction(SmartAgent agent)
        {
            return new FunctionalAction(() => StartUse(agent), Wait, () => StopUse(agent));
        }

        protected override Vector3 GetTargetPosition(SmartAgent agent)
        {
            return _targetTransform.position;
        }

        void StartUse(SmartAgent smartAgent)
        {
            lieTime = Time.time;
            _owner = smartAgent;
            smartAgent.transform.SetPositionAndRotation(_useTransform.position, _useTransform.rotation);
            _particleSystem.Play();
        }

        void StopUse(SmartAgent smartAgent)
        {
            smartAgent.transform.SetLocalPositionAndRotation(_targetTransform.position, _targetTransform.rotation);
            _particleSystem.Stop();
            _owner = null;
        }

        Status Wait()
        {
            if (Time.time > lieTime + useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}

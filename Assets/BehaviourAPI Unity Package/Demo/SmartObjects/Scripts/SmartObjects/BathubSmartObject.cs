using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAPI.Unity.Demos
{
    using Core;

    public class BathubSmartObject : SmartObject
    {
        [SerializeField] Transform _targetTransform;
        [SerializeField] Transform _useTransform;
        [SerializeField] float useTime = 5f;

        [SerializeField] ParticleSystem _particleSystem;

        float lieTime;

        public override void OnCompleteWithFailure(SmartAgent agent)
        {
        }

        public override void OnCompleteWithSuccess(SmartAgent agent)
        {
        }

        protected override Action GetRequestedAction(SmartAgent agent)
        {
            var liedown = new FunctionalAction(() => BedDown(agent), Wait, () => BedUp(agent));
            return liedown;
        }

        public override bool ValidateAgent(SmartAgent agent)
        {
            return true;
        }

        void BedDown(SmartAgent smartAgent)
        {
            _particleSystem.Play();
            lieTime = Time.time;
            smartAgent.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            smartAgent.transform.SetLocalPositionAndRotation(_useTransform.position, _useTransform.rotation);
        }

        Status Wait()
        {
            if (Time.time > lieTime + useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }

        void BedUp(SmartAgent smartAgent)
        {
            _particleSystem?.Stop();
            smartAgent.transform.SetLocalPositionAndRotation(_targetTransform.position, _targetTransform.rotation);
            smartAgent.gameObject.GetComponent<NavMeshAgent>().enabled = enabled;
        }

        protected override Vector3 GetTargetPosition(SmartAgent agent)
        {
            return _targetTransform.position;
        }
    }

}
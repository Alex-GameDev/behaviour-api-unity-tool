using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    using Core;

    public class ToiletSmartObject : SmartObject
    {
        [SerializeField] Transform _targetTransform;
        [SerializeField] Transform _useTransform;
        [SerializeField] float useTime = 5f;

        SmartAgent _owner;

        NPCPoseController _poseController;

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
            var sit = new FunctionalAction(() => SitDown(agent), Wait, () => SitUp(agent));
            return sit;
        }

        protected override Vector3 GetTargetPosition(SmartAgent agent)
        {
            return _targetTransform.position;
        }

        void SitDown(SmartAgent smartAgent)
        {
            lieTime = Time.time;
            _owner = smartAgent;
            _poseController = smartAgent.gameObject.GetComponent<NPCPoseController>();
            _poseController.ChangeToSittingPose();
            smartAgent.transform.SetPositionAndRotation(_useTransform.position, _useTransform.rotation);
        }

        void SitUp(SmartAgent smartAgent)
        {
            smartAgent.transform.SetLocalPositionAndRotation(_targetTransform.position, _targetTransform.rotation);
            _poseController?.ChangeToReleasePose();
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
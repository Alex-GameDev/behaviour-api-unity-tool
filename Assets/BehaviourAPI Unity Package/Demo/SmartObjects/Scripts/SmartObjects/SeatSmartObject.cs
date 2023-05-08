using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    public class SeatSmartObject : DirectSmartObject
    {
        private static float k_DefaultUseTime = 5f;

        [Tooltip("The position where the agent")]
        [SerializeField] Transform _useTarget;

        float _useTime = k_DefaultUseTime;

        NPCPoseController _poseController;

        float lieTime;

        public float UseTime { get => _useTime; set => _useTime = value; }

        protected override Action GetUseAction(SmartAgent agent)
        {
            Action action = new FunctionalAction(() => SitDown(agent), Wait, () => SitUp(agent));
            return action;
        }

        void SitDown(SmartAgent smartAgent)
        {
            lieTime = Time.time;
            _poseController = smartAgent.gameObject.GetComponent<NPCPoseController>();
            _poseController.ChangeToSittingPose();
            smartAgent.transform.SetPositionAndRotation(_useTarget.position, _useTarget.rotation);
        }

        void SitUp(SmartAgent smartAgent)
        {
            if (smartAgent == null && _placeTarget != null) return;

            smartAgent.transform.SetLocalPositionAndRotation(_placeTarget.position, _placeTarget.rotation);
            _poseController?.ChangeToReleasePose();
            _useTime = k_DefaultUseTime;
        }

        Status Wait()
        {
            if (Time.time > lieTime + _useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }
}
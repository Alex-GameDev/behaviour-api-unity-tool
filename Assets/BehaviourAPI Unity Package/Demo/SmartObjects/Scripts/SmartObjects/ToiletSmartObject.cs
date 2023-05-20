using BehaviourAPI.Core.Actions;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.Demos
{
    using Core;

    public class ToiletSmartObject : DirectSmartObject
    {
        [Tooltip("The position where the agent")]
        [SerializeField] Transform _useTarget;

        [SerializeField] float _useTime = 5f;

        NPCPoseController _poseController;

        float lieTime;

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
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.SmartObjects;
using BehaviourAPI.UnityToolkit;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.Demos
{
    public class BathroomDrawerSmartObject : DirectSmartObject
    {
        [SerializeField] ParticleSystem _particleSystem;
        [SerializeField] float useTime = 5f;

        float startTime;

        protected override Action GetUseAction(SmartAgent agent, RequestData requestData)
        {
            return new FunctionalAction(() => StartUse(agent), Wait, () => StopUse(agent));
        }

        void StartUse(SmartAgent smartAgent)
        {
            smartAgent.transform.SetPositionAndRotation(_placeTarget.position, _placeTarget.rotation);
            startTime = Time.time;
            _particleSystem.Play();
        }

        void StopUse(SmartAgent smartAgent)
        {
            _particleSystem?.Stop();
        }

        Status Wait()
        {
            if (Time.time > startTime + useTime)
            {
                return Status.Success;
            }
            return Status.Running;
        }
    }

}
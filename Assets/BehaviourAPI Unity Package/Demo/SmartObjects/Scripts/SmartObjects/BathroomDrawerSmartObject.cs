using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class BathroomDrawerSmartObject : SmartObject
{
    [SerializeField] Transform _targetTransform;
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] float useTime = 5f;

    SmartAgent _owner;

    float startTime;

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
        smartAgent.transform.SetPositionAndRotation(_targetTransform.position, _targetTransform.rotation);

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

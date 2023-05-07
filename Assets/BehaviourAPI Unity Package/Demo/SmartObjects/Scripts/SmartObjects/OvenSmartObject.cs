using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

public class OvenSmartObject : SmartObject
{
    [SerializeField] float useTime = 3f;
    [SerializeField] Transform _target;

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
        return _owner != agent;

    }

    protected override Action GetRequestedAction(SmartAgent agent)
    {
        return new FunctionalAction(() => lieTime = Time.time, () => OnUpdate(agent));
    }

    protected override Vector3 GetTargetPosition(SmartAgent agent)
    {
        return _target.position;
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

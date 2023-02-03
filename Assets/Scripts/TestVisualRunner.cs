using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

public class TestVisualRunner : VisualBehaviourRunner
{
    PushPerception _pp;
    protected override void OnStart()
    {
        _pp = FindPushPerception("push");
        base.OnStart();       
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _pp.Fire();
        }
    }

    [CustomMethod]
    public void foo() { }


    [CustomMethod]
    public Status St() => Status.Success;
}

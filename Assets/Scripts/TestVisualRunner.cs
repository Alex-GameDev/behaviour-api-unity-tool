using BehaviourAPI.Core.Perceptions;
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
}

using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

public class TestVisualRunner : VisualBehaviourRunner
{
    protected override void OnStart()
    {
        base.OnStart();

        FindPushPerception("pp");
        FindPushPerception("pp2");
        FindPushPerception("pp3");
        FindPushPerception("pp4");
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
    }

    [CustomMethod]
    public void foo() { }


    [CustomMethod]
    public Status St() => Status.Success;
}

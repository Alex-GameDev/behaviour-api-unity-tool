using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

public class TestVisualRunner : EditorBehaviourRunner
{
    [CustomMethod]
    public void foo() { }


    [CustomMethod]
    public Status St() => Status.Success;
}

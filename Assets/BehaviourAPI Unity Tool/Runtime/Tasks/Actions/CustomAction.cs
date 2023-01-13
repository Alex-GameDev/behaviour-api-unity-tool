using BehaviourAPI.Core;
using UnityEngine;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomAction : Action
    {
        [SerializeField] SerializedAction start;
        [SerializeField] SerializedStatusFunction update;
        [SerializeField] SerializedAction stop;

        public override void Start() => start.GetFunction()?.Invoke();

        public override void Stop() => stop.GetFunction()?.Invoke();

        public override Status Update() => update.GetFunction()?.Invoke() ?? Status.Running;
    }
}

using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomPerception : Perception
    {
        public SerializedAction init;
        public SerializedBoolFunction check;
        public SerializedAction reset;

        public override void Initialize() => init.GetFunction()?.Invoke();
        public override void Reset() => reset.GetFunction()?.Invoke();
        public override bool Check() => check.GetFunction()?.Invoke() ?? false;

    }
}

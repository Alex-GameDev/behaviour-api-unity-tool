using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CustomPerception : Perception
    {
        public Component component;
        public string MethodName;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Reset()
        {
            base.Reset();
        }
        public override bool Check()
        {
            throw new System.NotImplementedException();
        }

    }
}

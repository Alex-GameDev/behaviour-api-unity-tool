using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    [Serializable]
    public class PerceptionWrapper : Perception
    {
        [SerializeReference] public Perception perception;

        public override void Initialize() => perception.Initialize();

        public override bool Check() => perception.Check();

        public override void Reset() => perception.Reset();

        public override object Clone()
        {
            var copy = (PerceptionWrapper)base.Clone();
            copy.perception = (Perception)perception.Clone();
            return copy;
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            perception.SetExecutionContext(context);
        }
    }
}
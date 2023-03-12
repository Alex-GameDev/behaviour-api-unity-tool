using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class CompoundPerceptionWrapper : Perception, IBuildable
    {
        [SerializeReference] public CompoundPerception compoundPerception;

        public List<PerceptionWrapper> subPerceptions;

        public CompoundPerceptionWrapper()
        {
            subPerceptions = new List<PerceptionWrapper>();
        }

        public CompoundPerceptionWrapper(CompoundPerception compoundPerception)
        {
            this.compoundPerception = compoundPerception;
            subPerceptions = new List<PerceptionWrapper>();
        }

        public override void Initialize() => compoundPerception.Initialize();

        public override bool Check() => compoundPerception.Check();

        public override void Reset() => compoundPerception.Reset();

        public override object Clone()
        {
            var copy = (CompoundPerceptionWrapper)base.Clone();
            copy.compoundPerception = (CompoundPerception)compoundPerception.Clone();
            return copy;
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            compoundPerception.SetExecutionContext(context);
        }

        public void Build(SystemData data)
        {
            compoundPerception.Perceptions = subPerceptions.Select(p => p.perception).ToList();
        }


    }
}

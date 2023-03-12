using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.UnityTool.Framework
{
    public class CompoundPerceptionWrapper : Perception, IBuildable
    {
        CompoundPerception compoundPerception;

        public List<PerceptionWrapper> subPerceptions = new List<PerceptionWrapper>();

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

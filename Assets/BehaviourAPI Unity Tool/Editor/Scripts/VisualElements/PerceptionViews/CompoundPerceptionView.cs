using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompoundPerception = BehaviourAPI.Unity.Runtime.CompoundPerception;

namespace BehaviourAPI.Unity.Editor
{
    public class CompoundPerceptionView : PerceptionView<CompoundPerception>
    {
        public CompoundPerceptionView(CompoundPerception perception) : base(perception, BehaviourAPISettings.instance.CompoundPerceptionLayout)
        {
        }

        protected override void AddLayout()
        {

        }
    }
}

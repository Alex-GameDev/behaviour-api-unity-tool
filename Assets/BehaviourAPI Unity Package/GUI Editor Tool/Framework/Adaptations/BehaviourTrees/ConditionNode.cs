using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode, IBuildable, IPerceptionAssignable
    {
        [SerializeReference] public Perception perception;
        public Perception PerceptionReference
        {
            get => perception;
            set => perception = value;
        }
        public override object Clone()
        {
            var copy = (ConditionNode)base.Clone();
            copy.perception = (Perception)perception?.Clone();
            return copy;
        }

        public void Build(SystemData data)
        {
            if (perception is IBuildable buildable) buildable.Build(data);
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception;
        }
    }
}

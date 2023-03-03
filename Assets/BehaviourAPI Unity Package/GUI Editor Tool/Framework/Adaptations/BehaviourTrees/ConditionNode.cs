using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using System.Collections.Generic;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode, IPerceptionAssignable
    {
        public PerceptionAsset perception;

        public PerceptionAsset PerceptionReference
        {
            get => perception;
            set => perception = value;
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception?.perception;
        }
    }
}

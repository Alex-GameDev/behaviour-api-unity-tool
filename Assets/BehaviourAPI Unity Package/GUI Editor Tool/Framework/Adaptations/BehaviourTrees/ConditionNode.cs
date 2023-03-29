using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Perceptions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="BehaviourTrees.ConditionNode"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class ConditionNode : BehaviourTrees.ConditionNode, IBuildable, IPerceptionAssignable
    {
        /// <summary>
        /// Serializable Wrapper for <see cref="BehaviourTrees.ConditionNode.Perception"/>.
        /// </summary>
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

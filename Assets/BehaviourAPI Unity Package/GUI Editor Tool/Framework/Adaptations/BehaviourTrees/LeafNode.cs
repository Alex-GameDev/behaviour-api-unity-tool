using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Actions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="BehaviourTrees.LeafNode"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class LeafNode : BehaviourTrees.LeafNode, IActionAssignable, IBuildable
    {
        /// <summary>
        /// Serializable Wrapper for <see cref="BehaviourTrees.LeafNode.Action"/>.
        /// </summary>
        [SerializeReference] Action action;

        public Action ActionReference
        {
            get => action;
            set => action = value;
        }

        public override object Clone()
        {
            var copy = (LeafNode)base.Clone();
            copy.action = (Action)action?.Clone();
            return copy;
        }

        public void Build(SystemData data)
        {
            if (action is IBuildable buildable) buildable.Build(data);
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = action;
        }
    }
}
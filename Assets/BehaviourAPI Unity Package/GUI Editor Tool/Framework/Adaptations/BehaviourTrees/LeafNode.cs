using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class LeafNode : BehaviourTrees.LeafNode, IActionAssignable, IBuildable
    {
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
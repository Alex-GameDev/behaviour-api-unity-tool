using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class PopTransition : StateMachines.StackFSMs.PopTransition, IActionAssignable, IPerceptionAssignable
    {
        [SerializeReference] Action action;
        [SerializeReference] Perception perception;

        public Action ActionReference
        {
            get => action;
            set => action = value;
        }

        public Perception PerceptionReference
        {
            get => perception;
            set => perception = value;
        }

        public PopTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

        public override object Clone()
        {
            var copy = (PopTransition)base.Clone();
            copy.action = (Action)action?.Clone();
            copy.perception = (Perception)perception?.Clone();
            return copy;
        }

        public void Build(SystemData data)
        {
            if (action is IBuildable aBuildable) aBuildable.Build(data);
            if (perception is IBuildable pBuildable) pBuildable.Build(data);
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception;
            Action = action;
        }
    }
}
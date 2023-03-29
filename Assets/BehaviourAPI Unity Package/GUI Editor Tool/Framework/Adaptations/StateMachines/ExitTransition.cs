using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Actions;
    using Core.Perceptions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="StateMachines.ExitTransition"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    public class ExitTransition : StateMachines.ExitTransition, IActionAssignable, IPerceptionAssignable, IBuildable
    {
        /// <summary>
        /// Serializable Wrapper for <see cref="StateMachines.Transition.Action"/>.
        /// </summary>
        [SerializeReference] Action action;

        /// <summary>
        /// Serializable Wrapper for <see cref="StateMachines.Transition.Action"/>.
        /// </summary>
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

        /// <summary>
        /// The default value for flags will be Active.
        /// </summary>
        public ExitTransition()
        {
            StatusFlags = StatusFlags.Active;
        }

        public override object Clone()
        {
            var copy = (ExitTransition)base.Clone();
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

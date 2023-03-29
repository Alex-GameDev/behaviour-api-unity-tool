using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    using Core;
    using Core.Actions;

    /// <summary>
    /// Adaptation wrapper class for use <see cref="StateMachines.State"/> in editor tools. 
    /// <para>! -- Don't use this class directly in code.</para>
    /// </summary>
    /// 
    public class State : StateMachines.State, IActionAssignable, IBuildable
    {
        /// <summary>
        /// Serializable Wrapper for <see cref="StateMachines.State.Action"/>.
        /// </summary>
        [SerializeReference] Action action;

        public Action ActionReference
        {
            get => action;
            set => action = value;
        }

        public override object Clone()
        {
            var copy = (State)base.Clone();
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

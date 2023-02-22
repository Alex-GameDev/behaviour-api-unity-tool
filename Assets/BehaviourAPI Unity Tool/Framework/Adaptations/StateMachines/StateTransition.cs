using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class StateTransition : StateMachines.StateTransition, IActionAssignable, IPerceptionAssignable
    {
        [SerializeReference] Action _action;
        public PerceptionAsset perception;

        public Action ActionReference
        {
            get => _action;
            set => _action = value;
        }

        public PerceptionAsset PerceptionReference
        {
            get => perception;
            set => perception = value;
        }

        public StateTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception?.perception;
            Action = _action;
        }
    }
}

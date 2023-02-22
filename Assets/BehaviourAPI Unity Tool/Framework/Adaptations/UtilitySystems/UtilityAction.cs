using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class UtilityAction : UtilitySystems.UtilityAction, IActionAssignable
    {
        [SerializeReference] Action _action;

        public Action ActionReference
        {
            get => _action;
            set => _action = value;
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = _action;
        }
    }
}

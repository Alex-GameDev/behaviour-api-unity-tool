using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class PushTransition : StateMachines.StackFSMs.PushTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        public PerceptionAsset perception;

        public PushTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

        public void OnAfterDeserialize()
        {
            Action = _action;
        }

        public void OnBeforeSerialize()
        {
            return;
        }

        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Perception = perception?.perception;
        }
    }
}
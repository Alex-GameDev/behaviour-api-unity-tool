using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class FinishExecutionTransition : BehaviourAPI.StateMachines.StateTransition, ISerializationCallbackReceiver
    {
        public StatusFlags _statusFlags;
        [SerializeReference] Action _action;

        public void OnAfterDeserialize()
        {
            Action = _action;
        }

        public void OnBeforeSerialize()
        {
            return;
        }

        public override bool Check()
        {
            return ((uint)_sourceState.Status & (uint)_statusFlags) != 0;
        }
    }
}

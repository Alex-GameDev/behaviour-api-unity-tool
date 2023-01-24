using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
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

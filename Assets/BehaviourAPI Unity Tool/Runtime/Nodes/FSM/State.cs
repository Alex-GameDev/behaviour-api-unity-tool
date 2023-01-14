using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime.StateMachines
{
    public class State : BehaviourAPI.StateMachines.State, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;

        public void OnAfterDeserialize()
        {
            Action = _action;
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}

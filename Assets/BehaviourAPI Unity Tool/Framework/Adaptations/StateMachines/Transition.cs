using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class Transition : BehaviourAPI.StateMachines.StateTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeReference] Perception perception;

        public void OnAfterDeserialize()
        {
            Action = _action;
            Perception = perception;
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}

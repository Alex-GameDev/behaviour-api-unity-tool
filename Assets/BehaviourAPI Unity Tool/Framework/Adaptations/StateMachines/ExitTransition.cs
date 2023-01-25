using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ExitTransition : BehaviourAPI.StateMachines.ExitTransition, ISerializationCallbackReceiver
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

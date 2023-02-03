using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ExitTransition : StateMachines.ExitTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeReference] Perception perception;

        public StatusFlags StatusFlags;

        public void OnAfterDeserialize()
        {
            Action = _action;
            Perception = perception;
        }

        public void OnBeforeSerialize()
        {
            return;
        }

        public override bool Check()
        {
            if (perception != null) return base.Check();

            return ((uint)_sourceState.Status & (uint)StatusFlags) != 0;
        }
    }
}

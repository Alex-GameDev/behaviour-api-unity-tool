using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class StateTransition : StateMachines.StateTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeField] PerceptionAsset perception;

        public StatusFlags StatusFlags;

        public void OnAfterDeserialize()
        {
            Action = _action;

            if (perception != null)
            {
                Perception = perception.perception;
            }
        }

        public void OnBeforeSerialize()
        {
            return;
        }

        public override bool Check()
        {
            if (perception != null) return base.Check();

            return ((uint)_sourceState.Status & (uint) StatusFlags) != 0;
        }
    }
}

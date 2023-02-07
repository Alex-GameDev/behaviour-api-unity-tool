using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class StateTransition : StateMachines.StateTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeField] PerceptionAsset perception;

        public StateTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

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
    }
}

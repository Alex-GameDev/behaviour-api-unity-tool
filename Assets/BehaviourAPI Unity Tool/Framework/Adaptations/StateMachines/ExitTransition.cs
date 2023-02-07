using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ExitTransition : StateMachines.ExitTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeField] public PerceptionAsset perception;

        public ExitTransition()
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

using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class PopTransition : StateMachines.StackFSMs.PopTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeField] public PerceptionAsset perception;

        public PopTransition()
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
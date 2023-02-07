using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class PushTransition : StateMachines.StackFSMs.PushTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        [SerializeField] public PerceptionAsset perception;

        public PushTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

        public void OnAfterDeserialize()
        {
            Action = _action;

            if(perception != null)
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
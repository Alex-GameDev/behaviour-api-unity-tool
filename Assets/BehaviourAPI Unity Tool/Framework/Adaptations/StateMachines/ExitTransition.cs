using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ExitTransition : StateMachines.ExitTransition, ISerializationCallbackReceiver
    {
        [SerializeReference] Action _action;
        public PerceptionAsset perception;

        public ExitTransition()
        {
            StatusFlags = StatusFlags.Actived;
        }

        public override bool Check()
        {
            return perception?.perception?.Check() ?? true;
        }

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

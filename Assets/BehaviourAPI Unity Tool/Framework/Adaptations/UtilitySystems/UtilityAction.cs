using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class UtilityAction : UtilitySystems.UtilityAction, ISerializationCallbackReceiver
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

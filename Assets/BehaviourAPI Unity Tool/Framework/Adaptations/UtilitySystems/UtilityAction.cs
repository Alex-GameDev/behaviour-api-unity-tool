using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Extensions
{
    public class UtilityAction : BehaviourAPI.UtilitySystems.UtilityAction, ISerializationCallbackReceiver
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

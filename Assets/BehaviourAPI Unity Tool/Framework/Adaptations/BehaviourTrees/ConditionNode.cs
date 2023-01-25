using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode, ISerializationCallbackReceiver
    {
        [SerializeReference] Perception _perception;

        public void OnAfterDeserialize()
        {
            Perception = _perception;
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}

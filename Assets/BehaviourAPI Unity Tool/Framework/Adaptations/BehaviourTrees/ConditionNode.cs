using BehaviourAPI.Core.Perceptions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class ConditionNode : BehaviourTrees.ConditionNode, ISerializationCallbackReceiver
    {
        [SerializeField] PerceptionAsset perception;

        public void OnAfterDeserialize()
        {
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

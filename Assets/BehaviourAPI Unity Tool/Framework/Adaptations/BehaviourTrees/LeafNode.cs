using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class LeafNode : BehaviourTrees.LeafNode, ISerializationCallbackReceiver
    {
        [SerializeReference] Action SerializedAction;

        public void OnAfterDeserialize()
        {
            Action = SerializedAction;
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}
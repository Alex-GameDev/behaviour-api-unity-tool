using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
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
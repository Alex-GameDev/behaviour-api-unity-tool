using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class LeafNode : BehaviourTrees.LeafNode
    {
        [SerializeReference] Action SerializedAction;

        public override void Start()
        {
            Action = SerializedAction;
            base.Start();
        }
    }
}
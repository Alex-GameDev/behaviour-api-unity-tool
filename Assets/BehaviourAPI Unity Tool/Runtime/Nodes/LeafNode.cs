using BehaviourAPI.Core.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class LeafNode : BehaviourTrees.LeafNode
    {
        [SerializeReference] new Action Action;
    }
}
using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    public class LeafNode : BehaviourTrees.LeafNode, IActionAssignable
    {
        [SerializeReference] Action SerializedAction;

        public Action ActionReference 
        { 
            get => SerializedAction; 
            set => SerializedAction = value; 
        }



        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = SerializedAction;
        }
    }
}
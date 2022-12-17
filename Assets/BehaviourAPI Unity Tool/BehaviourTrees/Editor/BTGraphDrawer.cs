using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomGraphDrawer(typeof(BehaviourTree))]
    public class BTGraphDrawer : CustomGraphDrawer
    {
        public override NodeView DrawNode(NodeAsset nodeAsset)
        {
            if(nodeAsset.Node == null) return null;

            var node = nodeAsset.Node;
            var nodeType = node.GetType();

            if (nodeType.IsAssignableFrom(typeof(BTNode))) return null;
            return new BTNodeView(nodeAsset);
        }
    }
}
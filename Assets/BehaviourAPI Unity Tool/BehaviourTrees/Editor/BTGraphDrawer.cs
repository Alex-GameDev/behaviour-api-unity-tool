using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Runtime;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomGraphDrawer(typeof(BehaviourTree))]
    public class BTGraphDrawer : CustomGraphDrawer
    {
        public override NodeView DrawNode(NodeAsset node)
        {
            var nodeView = new BTNodeView(node);
            return nodeView;
        }
    }
}
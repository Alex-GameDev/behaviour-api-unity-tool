using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(BehaviourTree))]
    public class BehaviourTreeGraphRenderer : GraphRenderer
    {

        public NodeView RootView;

        public override void BuildContextualMenu(NodeView nodeView, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("SetAsRootNode", _ => nodeView.GraphView.SetRootNode(nodeView), _ => DropdownMenuAction.Status.Normal);
        }

        public override Edge DrawEdge(NodeAsset src, NodeAsset tgt)
        {
            throw new System.NotImplementedException();
        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            throw new System.NotImplementedException();
        }
    }
}

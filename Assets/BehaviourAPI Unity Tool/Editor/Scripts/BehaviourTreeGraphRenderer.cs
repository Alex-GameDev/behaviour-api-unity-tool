using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;
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

        public override List<Port> GetValidPorts(UQueryState<Port> candidates, Port startPort)
        {
            List<Port> validPorts = new List<Port>();
            var startPortNodeView = (NodeView)startPort.node;

            var childs = startPortNodeView.Node.GetPathToLeaves();
            var parents = startPortNodeView.Node.GetPathFromRoot();

            candidates.ForEach(port =>
            {
                if (startPort.direction == port.direction) return; // Same port direction
                if (startPort.node == port.node) return; // Same node

                var portNodeView = (NodeView)port.node;
                if (portNodeView == null) return;

                if (startPort.direction == Direction.Input)
                {
                    if (!port.portType.IsAssignableFrom(startPort.portType)) return;
                    if (childs.Contains(portNodeView.Node)) return;
                }
                else
                {
                    if (!startPort.portType.IsAssignableFrom(port.portType)) return;
                    if (parents.Contains(portNodeView.Node)) return;
                }

                validPorts.Add(port);
            });
            return validPorts;
        }

        
    }
}

using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(FSM))]
    public class FSMGraphRenderer : GraphRenderer
    {
        string stateLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().StateLayout);
        string transitionLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().TransitionLayout);

        public NodeView StartNodeView;

        public override void BuildContextualMenu(NodeView nodeView, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set Entry State", 
                _ => SetStartNode(nodeView), 
                _ =>
                {
                    if(nodeView.Node.Node is State)
                    {
                        return nodeView == StartNodeView ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal;
                    }
                    return DropdownMenuAction.Status.Hidden;

                } );
        }

        public override Edge DrawEdge(NodeAsset src, NodeAsset tgt)
        {
            throw new System.NotImplementedException();
        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            var nodeView = new NodeView(asset, graphView, asset.Node is State ? stateLayout : transitionLayout);

            if (nodeView.Node.Node.MaxInputConnections != 0)
            {
                var capacity = nodeView.Node.Node.MaxInputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = nodeView.InstantiatePort(Orientation.Vertical, Direction.Input, capacity, nodeView.Node.Node.GetType());
                port.portName = "IN";
                port.style.flexDirection = FlexDirection.Column;
                //port.style.top = new StyleLength(7);
                nodeView.inputContainer.Add(port);
            }
            else
                nodeView.inputContainer.style.display = DisplayStyle.None;

            if (nodeView.Node.Node.MaxOutputConnections != 0)
            {
                var capacity = nodeView.Node.Node.MaxOutputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = nodeView.InstantiatePort(Orientation.Vertical, Direction.Output, capacity, nodeView.Node.Node.ChildType);
                port.portName = "OUT";
                port.style.flexDirection = FlexDirection.Column;
                //port.style.top = new StyleLength(7);
                nodeView.outputContainer.Add(port);
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;

            return nodeView;
        }

        public override List<Port> GetValidPorts(UQueryState<Port> ports, Port startPort)
        {
            List<Port> validPorts = new List<Port>();
            var startPortNodeView = (NodeView)startPort.node;

            var childs = startPortNodeView.Node.GetPathToLeaves();
            var parents = startPortNodeView.Node.GetPathFromRoot();

            ports.ForEach(port =>
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

        public override GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            return change;
        }

        public void SetStartNode(NodeView nodeView)
        {

        }
    }
}

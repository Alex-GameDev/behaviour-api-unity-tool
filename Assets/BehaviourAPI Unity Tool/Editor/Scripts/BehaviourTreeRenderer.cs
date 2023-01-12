using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(BehaviourTree))]
    public class BehaviourTreeRenderer : GraphRenderer
    {
        string btLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        NodeView _rootView;

        public override void DrawGraph(GraphAsset graphAsset)
        {
            graphAsset.Nodes.ForEach(node => DrawNode(node));
            graphAsset.Nodes.ForEach(node => DrawConnections(node));

            var firstNode = graphAsset.Nodes.FirstOrDefault();
            if(firstNode != null) ChangeRootNode(assetViewPairs[firstNode]);
        }

        public override void DrawConnections(NodeAsset asset)
        {
            foreach(NodeAsset child in asset.Childs)
            {
                Port srcPort = assetViewPairs[asset].OutputPort;
                Port tgtPort = assetViewPairs[child].InputPort;
                Edge edge = srcPort.ConnectTo(tgtPort);

                graphView.AddConnectionView(edge);
                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
            }
        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            // Crear nodo
            var nodeView = new NodeView(asset, graphView, btLayout);

            // Crear puertos
            if (nodeView.Node.Node.MaxInputConnections != 0)
            {
                var capacity = nodeView.Node.Node.MaxInputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = nodeView.InstantiatePort(Orientation.Vertical, Direction.Input, capacity, nodeView.Node.Node.GetType());
                port.portName = "";
                port.style.flexDirection = FlexDirection.Column;
                nodeView.inputContainer.Add(port);
            }
            else
                nodeView.inputContainer.style.display = DisplayStyle.None;

            if (nodeView.Node.Node.MaxOutputConnections != 0)
            {
                var capacity = nodeView.Node.Node.MaxOutputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = nodeView.InstantiatePort(Orientation.Vertical, Direction.Output, capacity, nodeView.Node.Node.ChildType);
                port.portName = "";
                port.style.flexDirection = FlexDirection.ColumnReverse;
                nodeView.outputContainer.Add(port);
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;

            // Crear menú
            nodeView.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Set Root Node", _ => { 
                    nodeView.DisconnectPorts(nodeView.inputContainer); 
                    ChangeRootNode(nodeView); 
                }, _ => DropdownMenuAction.Status.Normal);
            }));

            if (graphView.Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                nodeView.capabilities -= Capabilities.Movable;
            }


            assetViewPairs.Add(asset, nodeView);
            graphView.AddNodeView(nodeView);
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
            var rootNode = graphView.GraphAsset.Nodes.Find(n => n.Parents.Count == 0);

            if(rootNode != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(rootNode);
                var view = assetViewPairs[rootNode];
                ChangeRootNode(view);
            }            
            return change;
        }

        void ChangeRootNode(NodeView newStartNode)
        {   
            if(newStartNode == null || newStartNode.Node.Parents.Count > 0) return;

            if(_rootView != null)
            {
                _rootView.inputContainer.Enable();
                _rootView.RootElement.Disable();
            }

            _rootView = newStartNode;
            if (_rootView != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(_rootView.Node);

                _rootView.inputContainer.Disable();
                _rootView.RootElement.Enable();
            }
        }
    }
}

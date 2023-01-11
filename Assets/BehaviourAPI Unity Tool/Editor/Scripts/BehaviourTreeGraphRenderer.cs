using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(BehaviourTree))]
    public class BehaviourTreeGraphRenderer : GraphRenderer
    {
        string btLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

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
            var nodeView = new NodeView(asset, graphView, btLayout);

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

        // (!) Ejecutar después de haber borrado los nodos del grafo
        public override GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Si el nodo raíz ha sido borrado, asignar nodo raíz al primer nodo sin conexiones de entrada.

            var rootNode = graphView.GraphAsset.Nodes.Find(n => n.Parents.Count == 0);

            if(rootNode != null)
            {
                var view = (NodeView) graphView.nodes.ToList().Find(n => n is NodeView nodeView && nodeView.Node == rootNode);
                if (view != null)
                {
                    view.SetAsStartNode();
                }
            }            
            return change;
        }
    }
}

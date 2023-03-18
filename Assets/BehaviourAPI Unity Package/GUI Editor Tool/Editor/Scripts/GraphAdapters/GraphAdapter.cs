using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Allows editor tools to use specific graph type
    /// </summary>
    public abstract class GraphAdapter
    {
        #region ------------------- Rendering -------------------

        /// <summary>
        /// The main node types of the target graph
        /// </summary>
        public abstract List<Type> MainTypes { get; }

        /// <summary>
        /// The types that can't be used in the editor
        /// </summary>
        public abstract List<Type> ExcludedTypes { get; }

        /// <summary>
        /// Create a new node view in the graph view.
        /// </summary>
        protected abstract NodeView GetLayout(NodeData data, BehaviourGraphView graphView);

        /// <summary>
        /// Render the specific details in the node.
        /// </summary>
        protected abstract void DrawNodeDetails(NodeView node);

        /// <summary>
        /// Create the node context menu actions
        /// </summary>
        protected abstract void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt);

        /// <summary>
        /// Create the graph context menu actions in editor mode
        /// </summary>
        protected abstract void SetUpGraphEditorContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt);

        /// <summary>
        /// Draw the graph details
        /// </summary>
        protected abstract void DrawGraphDetails(GraphData data, BehaviourGraphView graphView);

        /// <summary>
        /// Call when an element of the graph changes
        /// </summary>
        protected abstract GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change);

        /// <summary>
        /// Draw a node view
        /// </summary>
        public void DrawNode(NodeData asset, BehaviourGraphView graphView)
        {
            var nodeView = GetLayout(asset, graphView);
            DrawNodeDetails(nodeView);
            nodeView.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendSeparator();
                menuEvt.menu.AppendAction("Debug (Node)", _ => DebugNode(asset));

                if (!BehaviourEditorWindow.Instance.IsRuntime) SetUpNodeContextMenu(nodeView, menuEvt);

                menuEvt.StopPropagation();

            }));

            if (graphView.Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                //nodeView.capabilities -= Capabilities.Movable;
            }

            graphView.AddNodeView(nodeView);
        }

        void DebugNode(NodeData data)
        {
            Debug.Log($"Name: {data.name}\nType: {data.node.TypeName()} / Pos: {data.position}\n" +
                $"Parents: {data.parentIds.Count} ({data.parentIds.Select(p => p).Join()})\n" +
                $"Childs: {data.childIds.Count} ({data.childIds.Select(p => p).Join()})");
        }

        void DebugGraph(GraphData data)
        {
            Debug.Log($"Name: {data.name}\nType: {data.graph.TypeName()}\n" +
                $"Nodes: {data.nodes.Count} ({data.nodes.Select(p => p.name).Join()})\n");
        }

        /// <summary>
        /// Draw all connections that begins in a data.
        /// </summary>
        public void DrawConnections(NodeData data, BehaviourGraphView graphView)
        {
            if (data.node != null && data.node.MaxInputConnections == 0) return;

            var sourceView = graphView.GetViewOf(data);
            var nodeIdMap = graphView.graphData.GetNodeIdMap();
            for (int i = 0; i < data.childIds.Count; i++)
            {
                string childId = data.childIds[i];
                NodeData child = nodeIdMap[childId];
                var childView = graphView.GetViewOf(child);
                var srcPort = sourceView.GetBestPort(childView, Direction.Output);
                var tgtPort = childView.GetBestPort(sourceView, Direction.Input);

                EdgeView edge = srcPort.ConnectTo<EdgeView>(tgtPort);
                graphView.AddConnectionView(edge);

                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
                sourceView.OnConnected(edge, childView, srcPort, ignoreConnection: true);
                childView.OnConnected(edge, sourceView, tgtPort, ignoreConnection: true);
            }
        }

        /// <summary>
        /// Draw a graph
        /// </summary>
        public void DrawGraph(GraphData data, BehaviourGraphView graphView)
        {
            foreach (var node in data.nodes)
            {
                DrawNode(node, graphView);
            }

            foreach (var node in data.nodes)
            {
                DrawConnections(node, graphView);
            }
            DrawGraphDetails(data, graphView);
        }

        /// <summary>
        /// Create the graph context menu actions in any mode
        /// </summary>
        public void BuildGraphContextualMenu(ContextualMenuPopulateEvent menuEvt, BehaviourGraphView graphView)
        {
            menuEvt.menu.AppendSeparator();
            menuEvt.menu.AppendAction("Refresh", _ => graphView.RefreshView());
            menuEvt.menu.AppendAction("Auto layout", _ => AutoLayoutGraph(graphView));
            menuEvt.menu.AppendAction("Debug (Graph)", _ => DebugGraph(graphView.graphData));

            if (!BehaviourEditorWindow.Instance.IsRuntime) SetUpGraphEditorContextMenu(graphView, menuEvt);

            menuEvt.StopPropagation();
        }

        private void AutoLayoutGraph(BehaviourGraphView graphView)
        {
            var asset = graphView.graphData;
            var layoutHandler = new LayoutHandler();
            layoutHandler.ComputeLayout(asset);
            graphView.RefreshView();
        }

        public List<Port> ValidatePorts(UQueryState<Port> ports, Port startPort, GraphData data, bool allowLoops)
        {
            List<Port> validPorts = new List<Port>();

            NodeView startNodeView = startPort.node as NodeView;

            if (startNodeView != null)
            {
                var bannedTargetPorts = allowLoops ? new HashSet<NodeData>() : data.GetChildPathing(startNodeView.Node);
                var bannedSourcePorts = allowLoops ? new HashSet<NodeData>() : data.GetParentPathing(startNodeView.Node);

                foreach (var port in ports)
                {
                    if (ValidatePort(startPort, port, bannedTargetPorts, bannedSourcePorts))
                    {
                        validPorts.Add(port);
                    }
                }
            }
            return validPorts;
        }

        private bool ValidatePort(Port startPort, Port port, HashSet<NodeData> bannedTargetPorts, HashSet<NodeData> bannedSourcePorts)
        {
            if (startPort.direction == port.direction) return false;    // Same port direction
            if (startPort.node == port.node) return false;              // Same data

            var node = port.node as NodeView;
            if (node == null) return false;                             // view without data assigned

            if (startPort.direction == Direction.Input)
            {
                if (!port.portType.IsAssignableFrom(startPort.portType)) return false;      // Type missmatch
                if (bannedTargetPorts.Contains(node.Node)) return false;                   // Loop
            }
            else
            {
                if (!startPort.portType.IsAssignableFrom(port.portType)) return false;     // Type missmatch
                if (bannedSourcePorts.Contains(node.Node)) return false;                   // Loop
            }

            return true;
        }

        public GraphViewChange OnViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            return ViewChanged(graphView, change);
        }

        #endregion

        #region ----------------- Static methods ----------------

        public static GraphAdapter FindAdapter(BehaviourGraph graph)
        {
            var type = BehaviourAPISettings.instance.GetAdapter(graph.GetType());

            if (type != null)
            {
                return (GraphAdapter)Activator.CreateInstance(type);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}

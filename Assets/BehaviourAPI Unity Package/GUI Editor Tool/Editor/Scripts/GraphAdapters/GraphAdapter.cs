using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public abstract List<Type> MainTypes { get; }
        public abstract List<Type> ExcludedTypes { get; }
        protected abstract NodeView GetLayout(NodeAsset asset, BehaviourGraphView graphView);
        protected abstract void SetUpDetails(NodeView node);
        protected abstract void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt);
        protected abstract void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt);
        protected abstract void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews);
        protected abstract GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change);

        /// <summary>
        /// Draw a node view
        /// </summary>
        public void DrawNode(NodeAsset asset, BehaviourGraphView graphView)
        {
            var nodeView = GetLayout(asset, graphView);
            SetUpDetails(nodeView);
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

        void DebugNode(NodeAsset asset)
        {
            Debug.Log($"Name: {asset.Name}\nType: {asset.Node.TypeName()} / Pos: {asset.Position}\n" +
                $"Parents: {asset.Parents.Count} ({asset.Parents.Select(p => p.Name).Join()})\n" +
                $"Childs: {asset.Childs.Count} ({asset.Childs.Select(p => p.Name).Join()})");            
        }

        void DebugGraph(GraphAsset asset)
        {
            Debug.Log($"Name: {asset.Name}\nType: {asset.Graph.TypeName()}\n" +
                $"Nodes: {asset.Nodes.Count} ({asset.Nodes.Select(p => p.Name).Join()})\n");
        }

        /// <summary>
        /// Draw all connections that begins in a node.
        /// </summary>
        public void DrawConnections(NodeAsset asset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {
            if (asset.Node != null && asset.Node.MaxOutputConnections == 0) return;

            var sourceView = graphView.GetViewOf(asset);

            foreach (NodeAsset child in asset.Childs)
            {
                var targetView = graphView.GetViewOf(child);

                var srcPort = sourceView.GetBestPort(targetView, Direction.Output);
                var tgtPort = targetView.GetBestPort(sourceView, Direction.Input);

                EdgeView edge = srcPort.ConnectTo<EdgeView>(tgtPort);
                graphView.AddConnectionView(edge);

                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
                sourceView.OnConnected(targetView, srcPort, ignoreConnection: true);
                targetView.OnConnected(sourceView, tgtPort, ignoreConnection: true);
            }
        }

        /// <summary>
        /// Draw a graph
        /// </summary>
        public void DrawGraph(GraphAsset graphAsset, BehaviourGraphView graphView)
        {
            graphAsset.Nodes.ForEach(node => DrawNode(node, graphView));

            var nodeViews = graphView.nodes.Select(n => n as NodeView).ToList();

            graphAsset.Nodes.ForEach(node => DrawConnections(node, graphView, nodeViews));            

            DrawGraphDetails(graphAsset, graphView, nodeViews);           
        }

        public void BuildGraphContextualMenu(ContextualMenuPopulateEvent menuEvt, BehaviourGraphView graphView)
        {
            menuEvt.menu.AppendSeparator();
            menuEvt.menu.AppendAction("Refresh", _ => graphView.RefreshView());
            menuEvt.menu.AppendAction("Auto layout", _ => AutoLayoutGraph(graphView));
            menuEvt.menu.AppendAction("Debug (Graph)", _ => DebugGraph(graphView.GraphAsset));

            if (!BehaviourEditorWindow.Instance.IsRuntime)  SetUpGraphContextMenu(graphView, menuEvt);

            menuEvt.StopPropagation();
        }

        private void AutoLayoutGraph(BehaviourGraphView graphView)
        {
            var asset = graphView.GraphAsset;
            LayoutUtilities.ComputeLayout(asset);
            graphView.RefreshView();
        }

        /// <summary>
        /// Get the valid ports for a new connection
        /// </summary>
        public List<Port> GetValidPorts(UQueryState<Port> ports, Port startPort, bool allowLoops)
        {
            List<Port> validPorts = new List<Port>();
            var startPortNodeView = startPort.node as NodeView;

            if (startPortNodeView == null) return validPorts;

            var bannedTargetPorts = allowLoops ? new HashSet<NodeAsset>() : startPortNodeView.Node.GetPathToLeaves();
            var bannedSourcePorts = allowLoops ? new HashSet<NodeAsset>() : startPortNodeView.Node.GetPathFromRoot();

            foreach(var port in ports)
            {
                if (ValidatePort(startPort, port, bannedTargetPorts, bannedSourcePorts))
                {
                    validPorts.Add(port);
                }
            }
            return validPorts;
        }

        /// <summary>
        /// Return a tree entry list with the hierarchy of classes from the current graph class
        /// </summary>
        public List<SearchTreeEntry> GetNodeHierarchyEntries()
        {
            List<SearchTreeEntry> entries = new List<SearchTreeEntry>();

            entries.Add(new SearchTreeGroupEntry(new GUIContent("Nodes")));

            foreach (var type in MainTypes)
            {  
                var subtypes = TypeUtilities.GetSubClasses(type, includeSelf: true, excludeAbstract: true).Except(ExcludedTypes).ToList();
                if (subtypes.Count == 1)
                {
                    entries.AddEntry(subtypes[0].Name.CamelCaseToSpaced(), 1, subtypes[0]);
                }
                else if (subtypes.Count > 1)
                {
                    entries.AddGroup(type.Name.CamelCaseToSpaced() + "(s)", 1);
                    foreach (var subtype in subtypes)
                    {
                        entries.AddEntry(subtype.Name.CamelCaseToSpaced(), 2, subtype);
                    }
                }
            }
            return entries;
        }

        public GraphViewChange OnViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            return ViewChanged(graphView, change);
        }

        bool ValidatePort(Port startPort, Port port, HashSet<NodeAsset> bannedTargetPorts, HashSet<NodeAsset> bannedSourcePorts)
        {
            if (startPort.direction == port.direction) return false;    // Same port direction
            if (startPort.node == port.node) return false;              // Same node

            var node = port.node as NodeView;
            if (node == null) return false;                             // view without node assigned

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

        #endregion

        #region ----------------- Static methods ----------------

        public static GraphAdapter FindAdapter(BehaviourGraph graph)
        {
            var types = BehaviourAPISettings.instance.GetTypes().FindAll(t =>
               t.IsSubclassOf(typeof(GraphAdapter)) &&
               t.GetCustomAttributes().Any(a => a is CustomAdapterAttribute crAttrib && crAttrib.type == graph.GetType()));

            if (types.Count() > 0) return Activator.CreateInstance(types[0]) as GraphAdapter;
            else return null;
        }

        #endregion
    }
}

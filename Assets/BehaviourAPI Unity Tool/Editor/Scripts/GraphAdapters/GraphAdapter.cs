using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;

using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Action = BehaviourAPI.Core.Actions.Action;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Allows editor tools to use specific graph type
    /// </summary>
    public abstract class GraphAdapter
    {
        //TODO: Opciones para generar código
        // - Acciones y percepciones en la misma linea
        // - Incluir acciones aunque no sean necesarias

        #region --------------- Assets generation ---------------

        public abstract GraphAsset ConvertCodeToAsset(BehaviourGraph graph);

        #endregion

        #region ---------------- Code generation ----------------

        public abstract void ConvertAssetToCode(GraphAsset graphAsset, ScriptTemplate scriptTemplate);

        public abstract string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate, string graphName);

        protected string GenerateActionCode(Action action, ScriptTemplate scriptTemplate)
        {
            if (action is CustomAction customAction)
            {
                var parameters = new List<string>();

                var startMethodArg = GenerateSerializedMethodCode(customAction.start, scriptTemplate);
                var updateMethodArg = GenerateSerializedMethodCode(customAction.update, scriptTemplate);
                var stopMethodArg = GenerateSerializedMethodCode(customAction.stop, scriptTemplate);

                if (startMethodArg != null) parameters.Add(startMethodArg);
                if (updateMethodArg != null) parameters.Add(updateMethodArg);
                else parameters.Add("() => Status.Running");
                if (stopMethodArg != null) parameters.Add(stopMethodArg);

                return $"new {nameof(FunctionalAction)}({string.Join(", ", parameters)})";

            }
            else if (action is UnityAction unityAction)
            {
                // Add arguments
                return $"new {unityAction.TypeName()}( )";
            }
            else if (action is SubgraphAction subgraphAction)
            {
                var graphName = scriptTemplate.FindVariableName(subgraphAction.Subgraph);
                return $"new {nameof(SubsystemAction)}({graphName ?? "null /* Missing subgraph */"})";
            }
            else
                return null;
        }
        protected string GeneratePerceptionCode(Perception perception, ScriptTemplate scriptTemplate)
        {
            if (perception is CustomPerception customPerception)
            {
                var parameters = new List<string>();

                var initMethodArg = GenerateSerializedMethodCode(customPerception.init, scriptTemplate);
                var checkMethodArg = GenerateSerializedMethodCode(customPerception.check, scriptTemplate);
                var resetMethodArg = GenerateSerializedMethodCode(customPerception.reset, scriptTemplate);

                if (initMethodArg != null) parameters.Add(initMethodArg);
                if (checkMethodArg != null) parameters.Add(checkMethodArg);
                else parameters.Add("() => false");
                if (resetMethodArg != null) parameters.Add(resetMethodArg);
               
                return $"new {nameof(ConditionPerception)}({string.Join(", ", parameters)})";
            }
            else if (perception is UnityPerception unityPerception)
            {
                // Add arguments
                return $"new {unityPerception.TypeName()}()";
            }
            else
                return null;
        }

        protected string GenerateSerializedMethodCode(SerializedMethod serializedMethod, ScriptTemplate scriptTemplate)
        {
            if (serializedMethod != null && serializedMethod.component != null && !string.IsNullOrEmpty(serializedMethod.methodName))
            {
                var component = serializedMethod.component;
                var componentName = scriptTemplate.AddPropertyLine(component.TypeName(), component.TypeName().ToLower(), component);
                return $"{componentName}.{serializedMethod.methodName}";
            }
            else return null;
        }

        #endregion

        #region ------------------- Rendering -------------------

        protected abstract List<Type> MainTypes { get; }
        protected abstract List<Type> ExcludedTypes { get; }
        protected abstract string GetNodeLayoutPath(NodeAsset node);
        protected abstract void SetUpPortsAndDetails(NodeView node);
        protected abstract void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt);
        protected abstract void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt);
        protected abstract void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews);
        protected abstract GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change);

        /// <summary>
        /// Draw a node view
        /// </summary>
        public void DrawNode(NodeAsset asset, BehaviourGraphView graphView)
        {
            var nodeView = new NodeView(asset, graphView, GetNodeLayoutPath(asset));
            SetUpPortsAndDetails(nodeView);
            nodeView.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendSeparator();
                menuEvt.menu.AppendAction("Debug (Node)", _ => DebugNode(asset));
                SetUpNodeContextMenu(nodeView, menuEvt);
                menuEvt.StopPropagation();

            }));

            if (graphView.Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                nodeView.capabilities -= Capabilities.Movable;
            }

            graphView.AddNodeView(nodeView);
        }

        void DebugNode(NodeAsset asset)
        {
            Debug.Log($"Name: {asset.Name}\nType: {asset.Node.TypeName()}\n" +
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
            if (asset.Node.MaxOutputConnections == 0) return;

            //Port srcPort = nodeViews.Find(n => n.Node == asset).OutputPort;
            Port srcPort = graphView.GetViewOf(asset).OutputPort;

            foreach (NodeAsset child in asset.Childs)
            {
                //Port tgtPort = nodeViews.Find(n => n.Node == child).InputPort;
                Port tgtPort = graphView.GetViewOf(child).InputPort;
                Edge edge = srcPort.ConnectTo(tgtPort);
                graphView.AddConnectionView(edge);
                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
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
            menuEvt.menu.AppendAction("Debug (Graph)", _ => DebugGraph(graphView.GraphAsset));
            SetUpGraphContextMenu(graphView, menuEvt);
            menuEvt.StopPropagation();
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
                if (!startPort.portType.IsAssignableFrom(port.portType)) return false;      // Type missmatch
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

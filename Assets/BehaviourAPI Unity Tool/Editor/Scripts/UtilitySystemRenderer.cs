using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.BehaviourTrees.Decorators;
using BehaviourAPI.Unity.Editor.Assets.BehaviourAPI_Unity_Tool.Editor.Scripts.Utils;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UtilitySystems;
using BehaviourAPI.UtilitySystems.UtilityElements;
using BehaviourAPIUnityTool.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UtilityAction = BehaviourAPIUnityTool.UtilitySystems.UtilityAction;
using VariableFactor = BehaviourAPI.Unity.Runtime.VariableFactor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(UtilitySystem))]
    public class UtilitySystemRenderer : GraphRenderer
    {
        string selectableLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);
        string factorLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public override void DrawConnections(NodeAsset asset)
        {
            foreach (NodeAsset child in asset.Childs)
            {
                Port srcPort = assetViewPairs[asset].OutputPort;
                Port tgtPort = assetViewPairs[child].InputPort;
                Edge edge = srcPort.ConnectTo(tgtPort);

                graphView.AddConnectionView(edge);
                srcPort.node.RefreshPorts();
                tgtPort.node.RefreshPorts();
            }
        }

        public override void DrawGraph(GraphAsset graphAsset)
        {
            graphAsset.Nodes.ForEach(node => DrawNode(node));
            graphAsset.Nodes.ForEach(node => DrawConnections(node));
        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            // Crear nodo
            var nodeView = new NodeView(asset, graphView, selectableLayout);
            nodeView.Q("node-icon").Add(new Label(nodeView.Node.Node.GetType().Name.CamelCaseToSpaced().ToUpper()));

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
            return change;
        }

        public override List<SearchTreeEntry> GetNodeHierarchyEntries()
        {
            Type[] excludedTypes = new Type[] { typeof(CustomFunctionFactor) };

            List<SearchTreeEntry> entries = new List<SearchTreeEntry>();

            entries.Add(new SearchTreeGroupEntry(new GUIContent("US nodes")));

            entries.Add(GetTypeEntry(typeof(UtilityAction), 1));

            entries.Add(GetTypeEntry(typeof(UtilityBucket), 1));

            entries.Add(GetTypeEntry(typeof(UtilityExitNode), 1));

            entries.Add(new SearchTreeGroupEntry(new GUIContent("Fusion factor"), 1));
            var fusionTypes = TypeUtilities.GetSubClasses(typeof(FusionFactor), excludeAbstract: true).Except(excludedTypes).ToList();
            fusionTypes.ForEach(type => entries.Add(GetTypeEntry(type, 2)));

            entries.Add(new SearchTreeGroupEntry(new GUIContent("Function factor"), 1));
            var functionTypes = TypeUtilities.GetSubClasses(typeof(FunctionFactor), excludeAbstract: true).Except(excludedTypes).ToList();
            functionTypes.ForEach(type => entries.Add(GetTypeEntry(type, 2)));

            entries.Add(GetTypeEntry(typeof(VariableFactor), 1));

            return entries;
        }
    }
}

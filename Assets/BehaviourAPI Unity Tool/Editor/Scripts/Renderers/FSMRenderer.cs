using BehaviourAPI.BehaviourTrees.Decorators;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace BehaviourAPI.Unity.Editor
{
    [CustomRenderer(typeof(FSM))]
    public class FSMRenderer : GraphRenderer
    {
        string stateLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().StateLayout);
        string transitionLayout => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().TransitionLayout);

        NodeView _entryStateView;

        public override void DrawGraph(GraphAsset graphAsset)
        {
            graphAsset.Nodes.ForEach(node => DrawNode(node));
            graphAsset.Nodes.ForEach(node => DrawConnections(node));

            var firstState = graphAsset.Nodes.FirstOrDefault(n => n.Node is State);
            if(firstState != null) ChangeEntryState(assetViewPairs[firstState]);
        }

        public override NodeView DrawNode(NodeAsset asset)
        {
            var nodeView = new NodeView(asset, graphView, asset.Node is State ? stateLayout : transitionLayout);
            nodeView.Q("node-icon").Add(new Label(nodeView.Node.Node.GetType().Name.CamelCaseToSpaced().ToUpper()));

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

            

            // Crear menú
            nodeView.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Set Entry State",
                    _ => ChangeEntryState(nodeView),
                    _ => (nodeView.Node != null && nodeView.Node.Node is State) ?
                         (nodeView == _entryStateView) ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Hidden

                );
            }));

            assetViewPairs.Add(asset, nodeView);
            graphView.AddNodeView(nodeView);
            return nodeView;
        }

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

        public override List<Port> GetValidPorts(UQueryState<Port> ports, Port startPort)
        {
            List<Port> validPorts = new List<Port>();
            var startPortNodeView = (NodeView)startPort.node;

            ports.ForEach(port =>
            {
                if (startPort.direction == port.direction) return; // Same port direction
                if (startPort.node == port.node) return; // Same node

                var portNodeView = (NodeView)port.node;
                if (portNodeView == null) return;

                if (startPort.direction == Direction.Input)
                {
                    if (!port.portType.IsAssignableFrom(startPort.portType)) return;
                }
                else
                {
                    if (!startPort.portType.IsAssignableFrom(port.portType)) return;
                }

                validPorts.Add(port);
            });
            return validPorts;
        }

        public override GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            var rootNode = graphView.GraphAsset.Nodes.FirstOrDefault(n => n.Node is State);

            if (rootNode != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(rootNode);
                var view = assetViewPairs[rootNode];
                ChangeEntryState(view);
            }
            return change;
        }

        void ChangeEntryState(NodeView newStartNode)
        {
            if (newStartNode == null || newStartNode.Node.Node is not State) return;

            if (_entryStateView != null)
            {
                _entryStateView.RootElement.Disable();
            }

            _entryStateView = newStartNode;
            if (_entryStateView != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(_entryStateView.Node);
                _entryStateView.RootElement.Enable();
            }
        }

        public override List<SearchTreeEntry> GetNodeHierarchyEntries()
        {
            Type[] excludedTypes = new Type[] { typeof(State), typeof(ExitTransition), typeof(StateTransition), typeof(ProbabilisticState) };

            List<SearchTreeEntry> entries = new List<SearchTreeEntry>();

            entries.Add(new SearchTreeGroupEntry(new GUIContent("FSM nodes")));

            entries.Add(new SearchTreeGroupEntry(new GUIContent("States"), 1));
            var stateTypes = TypeUtilities.GetSubClasses(typeof(State), excludeAbstract: true).Except(excludedTypes).ToList();
            stateTypes.ForEach(type => entries.Add(GetTypeEntry(type, 2)));

            entries.Add(new SearchTreeGroupEntry(new GUIContent("Transitions"), 1));
            var transitionTypes = TypeUtilities.GetSubClasses(typeof(Transition), excludeAbstract: true).Except(excludedTypes).ToList();
            transitionTypes.ForEach(type => entries.Add(GetTypeEntry(type, 2)));

            return entries;
        }
    }
}

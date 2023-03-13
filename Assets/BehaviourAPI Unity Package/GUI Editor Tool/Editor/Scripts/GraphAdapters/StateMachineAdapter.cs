using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.StateMachines.StackFSMs;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using ExitTransition = BehaviourAPI.StateMachines.ExitTransition;
using ProbabilisticState = BehaviourAPI.StateMachines.ProbabilisticState;
using State = BehaviourAPI.StateMachines.State;
using StateTransition = BehaviourAPI.StateMachines.StateTransition;
using Transition = BehaviourAPI.StateMachines.Transition;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(FSM))]
    public class StateMachineAdapter : GraphAdapter
    {
        #region ------------------- Rendering -------------------

        NodeView _entryStateView;
        public override List<Type> MainTypes => new List<Type>
        {
            typeof(State),
            typeof(StateTransition),
            typeof(ExitTransition)
        };

        public override List<Type> ExcludedTypes => new List<Type> { 
            typeof(State), 
            typeof(ExitTransition), 
            typeof(StateTransition), 
            typeof(ProbabilisticState)
        };

        protected override NodeView GetLayout(NodeData asset, BehaviourGraphView graphView) => new CyclicNodeView(asset, graphView);

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order all node's child by position (x)", _ =>
            {
                graph.graphData.OrderAllChildNodes((n) => n.position.x);
                BehaviourEditorWindow.Instance.RegisterChanges();
            });
            menuEvt.menu.AppendAction("Order all node's child by position (y)", _ =>
            {
                graph.graphData.OrderAllChildNodes((n) => n.position.y);
                BehaviourEditorWindow.Instance.RegisterChanges();
            });
        }
        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set entry state", _ => ChangeEntryState(node), 
                _ => (node.Node != null && node.Node.node is State) ? (node != _entryStateView).ToMenuStatus() : DropdownMenuAction.Status.Hidden);
            menuEvt.menu.AppendAction("Order childs by position (x)",
                _ => node.GraphView.graphData.OrderChildNodes(node.Node, (n) => n.position.x),
                (node.Node.childIds.Count > 1).ToMenuStatus()
            );
            menuEvt.menu.AppendAction("Order childs by position (y)",
                _ => node.GraphView.graphData.OrderChildNodes(node.Node, (n) => n.position.y),
                (node.Node.childIds.Count > 1).ToMenuStatus()
            );
        }

        protected override void DrawGraphDetails(GraphData graphAsset, BehaviourGraphView graphView)
        {
            var firstState = graphAsset.nodes.FirstOrDefault(n => n.node is State);
            if (firstState != null) ChangeEntryState(graphView.nodeViews.Find(n => n.Node == firstState));
        }

        protected override void DrawNodeDetails(NodeView nodeView)
        {
            var contents = nodeView.Q("contents");
            if (nodeView.Node.node is Transition)
            {
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.TransitionColor);
                nodeView.Q("node-status").ChangeBorderColor(new Color(0,0,0,0));
                contents.style.width = 125;
            }
            else
            {
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.StateColor);
                contents.style.width = 200;
            }

            if (nodeView.Node.node is ExitTransition)
            {
                var label = nodeView.RootElement.Q<Label>("node-root-label");
                label.text = "Exit";
                nodeView.RootElement.Q("node-root-tag").ChangeBackgroundColor(new Color(.8f, .3f, .3f));

                nodeView.RootElement.Enable();
            }
        }

        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            var rootNode = graphView.graphData.nodes.FirstOrDefault(n => n.node is State);

            if (rootNode != null)
            {
                graphView.graphData.nodes.MoveAtFirst(rootNode);
                var view = graphView.nodes.Select(n => n as NodeView).ToList().Find(n => n.Node == rootNode);
                ChangeEntryState(view);
            }
            return change;
        }

        void ChangeEntryState(NodeView newStartNode)
        {
            if (newStartNode == null || newStartNode.Node.node is not State) return;

            var graphView = newStartNode.GraphView;

            if (_entryStateView != null)
            {
                _entryStateView.RootElement.Disable();
            }

            _entryStateView = newStartNode;
            if (_entryStateView != null)
            {
                graphView.graphData.nodes.MoveAtFirst(_entryStateView.Node);
                BehaviourEditorWindow.Instance.RegisterChanges();
                _entryStateView.RootElement.Enable();
            }
        }

        #endregion
    }

    public class CyclicNodeView : NodeView
    {
        public CyclicNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/CG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/CG Node.uxml";

        Port inputUniquePort, outputUniquePort;

        public override void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnConnected(edgeView, other, port, ignoreConnection);

            //Debug.Log("Disabling all ports except the connected one");
            if (port.direction == Direction.Input)
            {
                if (Node.node != null && Node.node.MaxInputConnections == 1)
                {
                    InputPorts.ForEach(p => { if (p != port) p.Disable(); });
                    inputUniquePort = port;
                }

            }
            else
            {
                if (Node.node != null && Node.node.MaxOutputConnections == 1)
                {
                    OutputPorts.ForEach(p => { if (p != port) p.Disable(); });
                    outputUniquePort = port;
                }
            }

            if (GraphView.Runtime && port.direction == Direction.Output)
            {
                if (other.Node.node is Transition t)
                {
                    t.SourceStateLastStatusChanged += (status) => edgeView.control.UpdateStatus(status);
                    edgeView.control.UpdateStatus(t.SourceStateLastStatus);
                }
            }
        }

        public override void OnDisconnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnDisconnected(edgeView, other, port, ignoreConnection);

            //Debug.Log("Enabling all ports");
            if (port.direction == Direction.Input)
            {
                if (Node.node != null && Node.node.MaxInputConnections == 1)
                {
                    InputPorts.ForEach(p => { if (p != port) p.Enable(); });
                    inputUniquePort = null;
                }

            }
            else
            {
                if (Node.node != null && Node.node.MaxOutputConnections == 1)
                {
                    OutputPorts.ForEach(p => { if (p != port) p.Enable(); });
                    outputUniquePort = null;
                }
            }
        }

        public override void SetUpPorts()
        {
            if (Node.node == null || Node.node.MaxInputConnections != 0)
            {
                var port1 = InstantiatePort(Direction.Input, PortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.left = new StyleLength(new Length(50, LengthUnit.Percent));

                var port2 = InstantiatePort(Direction.Input, PortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.top = new StyleLength(new Length(50, LengthUnit.Percent));

                var port3 = InstantiatePort(Direction.Input, PortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.right = new StyleLength(new Length(50, LengthUnit.Percent));

                var port4 = InstantiatePort(Direction.Input, PortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.node == null || Node.node.MaxOutputConnections != 0)
            {
                var port1 = InstantiatePort(Direction.Output, PortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.right = new StyleLength(new Length(50, LengthUnit.Percent));

                var port2 = InstantiatePort(Direction.Output, PortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));

                var port3 = InstantiatePort(Direction.Output, PortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.left = new StyleLength(new Length(50, LengthUnit.Percent));

                var port4 = InstantiatePort(Direction.Output, PortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
            }
            else
                outputContainer.style.display = DisplayStyle.None;
        }

        public override Port GetBestPort(NodeView other, Direction dir)
        {
            if (dir == Direction.Input)
            {
                if (inputUniquePort != null) return inputUniquePort;
                else
                {
                    if (InputPorts.Count < 4) return null;
                    var otherPos = other.Node.position;
                    var delta = otherPos - Node.position;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0) return InputPorts[1];
                        else return InputPorts[3];
                    }
                    else
                    {
                        if (delta.y > 0) return InputPorts[2];
                        else return InputPorts[0];
                    }
                }
            }
            else
            {
                if (outputUniquePort != null) return outputUniquePort;
                else
                {
                    if (OutputPorts.Count < 4) return null;
                    var otherPos = other.Node.position;
                    var delta = otherPos - Node.position;
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        if (delta.x > 0) return OutputPorts[1];
                        else return OutputPorts[3];
                    }
                    else
                    {
                        if (delta.y > 0) return OutputPorts[2];
                        else return OutputPorts[0];
                    }
                }
            }
        }
    }
}

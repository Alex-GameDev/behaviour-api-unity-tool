using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor.Graph
{
    [CustomNodeDrawer(typeof(FSMNode))]
    public class FSMNodeDrawer : NodeDrawer
    {
        List<PortView> InputPorts, OutputPorts;
        PortView inputUniquePort, outputUniquePort;

        VisualElement rootIcon;
        Label rootLabel;

        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/cyclicgraphnode.uxml";

        public override void DrawNodeDetails()
        {
            rootIcon = view.Q("node-root");
            switch (node)
            {
                case State:
                    view.SetColor(BehaviourAPISettings.instance.StateColor);
                    break;
                case StateTransition:
                    view.SetColor(BehaviourAPISettings.instance.TransitionColor);
                    view.AddExtensionView("statusFlags");
                    break;
                case ExitTransition:
                    view.SetColor(BehaviourAPISettings.instance.TransitionColor);
                    rootIcon.Enable();
                    view.AddExtensionView("statusFlags");
                    rootLabel = view.Q<Label>("node-root-label");
                    break;
            }

            RecomputeEntryNode();
            OnRepaint();
        }

        public override void OnConnected(EdgeView edgeView)
        {
            Direction dir = edgeView.input.node == view ? Direction.Input : Direction.Output;

            if (dir == Direction.Input && node.MaxInputConnections == 1)
            {
                inputUniquePort = edgeView.input as PortView;
                InputPorts.ForEach(p => { if (p != inputUniquePort) p.Disable(); });
            }
            else if (dir == Direction.Output && node.MaxOutputConnections == 1)
            {
                outputUniquePort = edgeView.output as PortView;
                OutputPorts.ForEach(p => { if (p != outputUniquePort) p.Disable(); });
            }

            if (edgeView.input.node != view || !view.graphView.IsRuntime) return;

            if (view.InputConnectionViews.Count == 1 && node is Transition transition)
            {
                transition.SourceStateLastStatusChanged += OnSourceStateLastStatusChanged;
                OnSourceStateLastStatusChanged(transition.SourceStateLastStatus);
            }
        }

        public override void OnDisconnected(EdgeView edgeView)
        {
            Direction dir = edgeView.input.node == view ? Direction.Input : Direction.Output;

            if (dir == Direction.Input && node.MaxInputConnections == 1)
            {
                inputUniquePort = null;
                InputPorts.ForEach(p => p.Enable());
            }
            else if (dir == Direction.Output && node.MaxOutputConnections == 1)
            {
                outputUniquePort = null;
                OutputPorts.ForEach(p => p.Enable());
            }
        }

        public override void OnDestroy()
        {
            if (view.graphView.IsRuntime && node is Transition transition && view.InputConnectionViews.Count == 1)
            {
                transition.SourceStateLastStatusChanged -= OnSourceStateLastStatusChanged;
            }
        }

        public override PortView GetPort(NodeView other, Direction dir)
        {
            if (dir == Direction.Input)
            {
                if (inputUniquePort != null) return inputUniquePort;
                else
                {
                    var otherPos = other.data.position;
                    var delta = otherPos - view.data.position;
                    int idx = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? delta.x > 0 ? 1 : 3 : delta.y > 0 ? 2 : 0;
                    return InputPorts[idx];
                }
            }
            else
            {
                if (outputUniquePort != null) return outputUniquePort;
                else
                {
                    var otherPos = other.data.position;
                    var delta = otherPos - view.data.position;
                    int idx = Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? delta.x > 0 ? 1 : 3 : delta.y > 0 ? 2 : 0;
                    return OutputPorts[idx];
                }
            }
        }

        public override void OnRepaint()
        {
            if (IsValidEntryNode(view.data))
            {
                if (view.graphView.graphData.nodes.First() == view.data)
                {
                    rootIcon.Enable();
                }
                else
                {
                    rootIcon.Disable();
                }
            }
        }

        public override void SetUpPorts()
        {
            InputPorts = new List<PortView>();
            OutputPorts = new List<PortView>();
            if (node == null || node.MaxInputConnections != 0)
            {
                var port1 = view.InstantiatePort(Direction.Input, EPortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.left = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port1);

                var port2 = view.InstantiatePort(Direction.Input, EPortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port2);

                var port3 = view.InstantiatePort(Direction.Input, EPortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.right = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port3);

                var port4 = view.InstantiatePort(Direction.Input, EPortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port4);
            }
            else
            {
                view.inputContainer.style.display = DisplayStyle.None;
            }

            if (node == null || node.MaxOutputConnections != 0)
            {
                var port1 = view.InstantiatePort(Direction.Output, EPortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.right = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port1);

                var port2 = view.InstantiatePort(Direction.Output, EPortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port2);

                var port3 = view.InstantiatePort(Direction.Output, EPortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.left = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port3);

                var port4 = view.InstantiatePort(Direction.Output, EPortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port4);
            }
            else
                view.outputContainer.style.display = DisplayStyle.None;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (view.graphView.IsRuntime) return;

            evt.menu.AppendAction("Convert to entry state",
               _ => ConvertToEntryState(),
               (view.GetDataIndex() != 0 && node is State) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Order childs/by x position", _ => view.OrderChildNodes(n => n.position.x),
                view.data.childIds.Count > 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendAction("Order childs/by y position", _ => view.OrderChildNodes(n => n.position.y),
                view.data.childIds.Count > 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        }

        public override void OnDeleted()
        {
            RecomputeEntryNode();
        }

        public override void OnMoved()
        {
            inputUniquePort = null;
            InputPorts.ForEach(p => p.Enable());

            outputUniquePort = null;
            OutputPorts.ForEach(p => p.Enable());

            foreach (var edgeView in view.OutputConnectionViews)
            {
                var other = edgeView.input.node as NodeView;

                var newOutputPort = view.GetBestPort(other, Direction.Output);
                if (newOutputPort != edgeView.output)
                {
                    edgeView.output.Disconnect(edgeView);
                    newOutputPort.Connect(edgeView);
                    edgeView.output = newOutputPort;
                }

                var newInputPort = other.GetBestPort(view, Direction.Input);
                if (newInputPort != edgeView.input)
                {
                    edgeView.input.Disconnect(edgeView);
                    newInputPort.Connect(edgeView);
                    edgeView.input = newInputPort;
                }
            }

            foreach (var edgeView in view.InputConnectionViews)
            {
                var other = edgeView.output.node as NodeView;

                var newOutputPort = other.GetBestPort(view, Direction.Output);
                if (newOutputPort != edgeView.output)
                {
                    edgeView.output.Disconnect(edgeView);
                    newOutputPort.Connect(edgeView);
                    edgeView.output = newOutputPort;
                }

                var newInputPort = view.GetBestPort(other, Direction.Input);
                if (newInputPort != edgeView.input)
                {
                    edgeView.input.Disconnect(edgeView);
                    newInputPort.Connect(edgeView);
                    edgeView.input = newInputPort;
                }
            }

        }

        public override void OnRefreshDisplay()
        {
            if (node is Transition t)
            {
                var flagsDisplay = t.StatusFlags.DisplayInfo();
                var taskView = view.GetTaskView("statusFlags");
                taskView.Update("Check " + flagsDisplay);
                if (t is ExitTransition exit)
                {
                    rootLabel.text = "Exit with " + exit.ExitStatus;
                }
            }
        }
        private void RecomputeEntryNode()
        {
            var nodes = view.graphView.graphData.nodes;
            if (nodes.Count > 0 && !IsValidEntryNode(nodes.First()))
            {
                var newRootNode = nodes.FirstOrDefault(IsValidEntryNode);

                if (newRootNode != null) nodes.MoveAtFirst(newRootNode);
            }
        }

        private void ConvertToEntryState()
        {
            view.ConvertToFirstNode();
        }

        private void OnSourceStateLastStatusChanged(Status status)
        {
            var edgeView = view.InputConnectionViews[0];
            edgeView.control.UpdateStatus(status);
        }

        private static bool IsValidEntryNode(NodeData data)
        {
            return data.node is State;
        }
    }
}

using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.UtilitySystems;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomNodeDrawer(typeof(FSMNode))]
    public class FSMNodeDrawer : NodeDrawer
    {
        List<PortView> InputPorts, OutputPorts;
        PortView inputUniquePort, outputUniquePort;

        VisualElement rootIcon;

        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/CG Node.uxml";

        public override void DrawNodeDetails()
        {
            rootIcon = view.Q("node-root");
            switch (node)
            {
                case State:
                    SetColor(BehaviourAPISettings.instance.StateColor);
                    break;
                case Transition:
                    SetColor(BehaviourAPISettings.instance.TransitionColor);
                    break;
            }


            RecomputeEntryNode();
            OnRepaint();
        }

        private void SetColor(Color color)
        {
            view.Find("node-type-color-top").ChangeBackgroundColor(color);
            view.Find("node-type-color-bottom").ChangeBackgroundColor(color);
        }

        private void SetIconText(string text)
        {
            var iconElement = view.Find("node-icon");
            iconElement.Enable();
            iconElement.Add(new Label(text));
        }

        public override PortView GetPort(MNodeView other, Direction dir)
        {
            if(dir == Direction.Input)
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
            if (view.graphView.graphData.nodes.First() == view.data && IsValidEntryNode(view.data))
            {
                rootIcon.Enable();
            }
            else
            {
                rootIcon.Disable();
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

        public override void OnDeleted()
        {
            RecomputeEntryNode();
        }

        private static bool IsValidEntryNode(NodeData data)
        {
            return data.node is State;
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

        public override void OnMoved()
        {
            foreach(var edgeView in view.OutputConnectionViews)
            {
                var other = edgeView.input.node as MNodeView;

                var newOutputPort = view.GetBestPort(other, Direction.Output);
                if(newOutputPort != edgeView.output)
                {
                    edgeView.output.Disconnect(edgeView);
                    newOutputPort.Connect(edgeView);
                    edgeView.output = newOutputPort;
                }

                var newInputPort = other.GetBestPort(view, Direction.Input);
                if(newInputPort != edgeView.input)
                {
                    edgeView.input.Disconnect(edgeView);
                    newInputPort.Connect(edgeView);
                    edgeView.input = newInputPort;
                }
            }

            foreach (var edgeView in view.InputConnectionViews)
            {
                var other = edgeView.output.node as MNodeView;

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
            switch (node)
            {
                case Transition t:
                    var flagsDisplay = t.StatusFlags.DisplayInfo();
                    view.CustomView.Update("Check when " + flagsDisplay);
                    break;
            }
        }
    }
}

using BehaviourAPI.StateMachines;
using System.Collections.Generic;
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

        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/CG Node.uxml";

        public override void DrawNodeDetails()
        {
        }

        public override PortView GetPort(MNodeView other, Direction dir)
        {
            if(dir == Direction.Input)
            {
                if (inputUniquePort != null) return inputUniquePort;
                else
                {
                    var otherPos = other.GetPosition().position;
                    var delta = otherPos - nodeView.GetPosition().position;
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
                    var otherPos = other.GetPosition().position;
                    var delta = otherPos - nodeView.GetPosition().position;
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

        public override void OnRepaint()
        {
        }

        public override void OnSelected()
        {
        }

        public override void OnUnselected()
        {
        }

        public override void SetUpPorts()
        {
            InputPorts = new List<PortView>();
            OutputPorts = new List<PortView>();
            if (node == null || node.MaxInputConnections != 0)
            {
                var port1 = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.left = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port1);

                var port2 = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port2);

                var port3 = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.right = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port3);

                var port4 = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
                InputPorts.Add(port4);
            }
            else
            {
                nodeView.inputContainer.style.display = DisplayStyle.None;
            }

            if (node == null || node.MaxOutputConnections != 0)
            {
                var port1 = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.right = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port1);

                var port2 = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port2);

                var port3 = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.left = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port3);

                var port4 = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
                OutputPorts.Add(port4);
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;
        }
    }
}

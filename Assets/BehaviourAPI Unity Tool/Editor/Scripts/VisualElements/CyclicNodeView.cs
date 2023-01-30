using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class CyclicNodeView : NodeView
    {
        public CyclicNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/CG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/CG Node.uxml";

        Port inputUniquePort, outputUniquePort;

        public override void OnConnected(NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnConnected(other, port, ignoreConnection);

            //Debug.Log("Disabling all ports except the connected one");
            if(port.direction == Direction.Input)
            {
                if(Node.Node.MaxInputConnections == 1)
                {
                    InputPorts.ForEach(p => { if (p != port) p.Disable(); });
                    inputUniquePort = port;
                }
                
            }
            else
            {
                if (Node.Node.MaxOutputConnections == 1)
                {
                    OutputPorts.ForEach(p => { if (p != port) p.Disable(); });
                    outputUniquePort = port;
                }
               
            }
        }

        public override void OnDisconnected(NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnDisconnected(other, port, ignoreConnection);

            //Debug.Log("Enabling all ports");
            if (port.direction == Direction.Input)
            {
                if (Node.Node.MaxInputConnections == 1)
                {
                    InputPorts.ForEach(p => { if (p != port) p.Enable(); });
                    inputUniquePort = null;
                }
                
            }
            else
            {
                if (Node.Node.MaxOutputConnections == 1)
                {
                    OutputPorts.ForEach(p => { if (p != port) p.Enable(); });
                    outputUniquePort = null;
                }               
            }
        }

        public override void SetUpPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
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

            if (Node.Node.MaxOutputConnections != 0)
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
            if(dir == Direction.Input)
            {
                if (inputUniquePort != null) return inputUniquePort;
                else
                {
                    if (InputPorts.Count < 4) return null;
                    var otherPos = other.Node.Position;
                    var delta = otherPos - Node.Position;
                    if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
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
                    var otherPos = other.Node.Position;
                    var delta = otherPos - Node.Position;
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

    public class LayeredNodeView : NodeView
    {
        public LayeredNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/DAG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/DAG Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Right);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.Node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Left);
            }
            else
                outputContainer.style.display = DisplayStyle.None;
        }
    }

    public class TreeNodeView : NodeView
    {
        public TreeNodeView(NodeAsset node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorElementPath + "/Nodes/Tree Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/Tree Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Bottom);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.Node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Top);
            }
            else
            {
                outputContainer.style.display = DisplayStyle.None;
            }
        }
    }
}

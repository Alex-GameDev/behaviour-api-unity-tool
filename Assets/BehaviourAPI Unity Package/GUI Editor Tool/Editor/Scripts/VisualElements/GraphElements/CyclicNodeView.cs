using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
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
        public CyclicNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/CG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/CG Node.uxml";

        Port inputUniquePort, outputUniquePort;

        public override void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnConnected(edgeView, other, port, ignoreConnection);

            //Debug.Log("Disabling all ports except the connected one");
            if(port.direction == Direction.Input)
            {
                if(Node.node != null && Node.node.MaxInputConnections == 1)
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
            if(dir == Direction.Input)
            {
                if (inputUniquePort != null) return inputUniquePort;
                else
                {
                    if (InputPorts.Count < 4) return null;
                    var otherPos = other.Node.position;
                    var delta = otherPos - Node.position;
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

    public class LayeredNodeView : NodeView
    {
        public LayeredNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/DAG Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/DAG Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.node == null || Node.node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Right);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.node == null || Node.node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Left);
            }
            else
                outputContainer.style.display = DisplayStyle.None;
        }
    }

    public class TreeNodeView : NodeView
    {
        public TreeNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/Tree Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/Tree Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.node == null || Node.node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Bottom);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.node == null || Node.node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Top);
            }
            else
            {
                outputContainer.style.display = DisplayStyle.None;
            }
        }

        public override void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnConnected(edgeView, other, port, ignoreConnection);

            if(GraphView.Runtime && port.direction == Direction.Output)
            {
                if (other.Node.node is BTNode btNode)
                {
                    btNode.LastExecutionStatusChanged += (status) => edgeView.control.UpdateStatus(status);
                    edgeView.control.UpdateStatus(btNode.LastExecutionStatus);
                }
            }
        }

        public void ResetStatus()
        {
            outputEdges.ForEach(edge =>
            {
                edge.control.UpdateStatus(Status.None);
                (edge.input.node as TreeNodeView).ResetStatus();
            });
        }
    }
}

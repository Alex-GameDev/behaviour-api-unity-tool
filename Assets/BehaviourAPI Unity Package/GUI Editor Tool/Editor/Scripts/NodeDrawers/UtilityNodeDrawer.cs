using BehaviourAPI.UtilitySystems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    [CustomNodeDrawer(typeof(UtilityNode))]
    public class UtilityNodeDrawer : NodeDrawer
    {
        PortView InputPort, OutputPort;

        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/DAG Node.uxml";

        public override void DrawNodeDetails()
        {
        }

        public override PortView GetPort(MNodeView nodeView, Direction direction)
        {
            if (direction == Direction.Input) return InputPort;
            else return OutputPort;
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
            if (node != null || node.MaxInputConnections != 0)
            {
                InputPort = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Right);
            }
            else nodeView.inputContainer.Disable();

            if (node != null || node.MaxOutputConnections != 0)
            {
                OutputPort = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Left);
            }
            else nodeView.outputContainer.Disable();
        }
    }
}

using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.UtilitySystems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomNodeDrawer(typeof(UtilityNode))]
    public class UtilityNodeDrawer : NodeDrawer
    {
        PortView InputPort, OutputPort;

        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/DAG Node.uxml";

        public override void DrawNodeDetails()
        {
            switch (node)
            {
                case VariableFactor:
                case ConstantFactor:
                    SetColor(BehaviourAPISettings.instance.LeafFactorColor);
                    break;
                case FusionFactor:
                    SetColor(BehaviourAPISettings.instance.FusionFactorColor);
                    SetIconText(node.TypeName().CamelCaseToSpaced());
                    break;
                case CurveFactor:
                    SetColor(BehaviourAPISettings.instance.CurveFactorColor);
                    SetIconText(node.TypeName().CamelCaseToSpaced());
                    break;
                case UtilityBucket:
                    SetColor(BehaviourAPISettings.instance.BucketColor);
                    break;
                case UtilityExecutableNode:
                    SetColor(BehaviourAPISettings.instance.SelectableNodeColor);
                    break;
            }
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
                InputPort = view.InstantiatePort(Direction.Input, EPortOrientation.Right);
            }
            else view.inputContainer.Disable();

            if (node != null || node.MaxOutputConnections != 0)
            {
                OutputPort = view.InstantiatePort(Direction.Output, EPortOrientation.Left);
            }
            else view.outputContainer.Disable();
        }
    }
}

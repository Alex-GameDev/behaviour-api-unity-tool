using BehaviourAPI.BehaviourTrees;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomNodeDrawer(typeof(BTNode))]
    public class BTNodeDrawer : NodeDrawer
    {
        PortView InputPort, OutputPort;
        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/Tree Node.uxml";

        public override void DrawNodeDetails()
        {
            switch(node)
            {
                case LeafNode:
                    SetColor(BehaviourAPISettings.instance.LeafNodeColor);
                    break;
                case CompositeNode:
                    SetColor(BehaviourAPISettings.instance.CompositeColor);
                    SetIconText(node.TypeName().CamelCaseToSpaced());
                    break;
                case DecoratorNode:
                    SetColor(BehaviourAPISettings.instance.DecoratorColor);
                    SetIconText(node.TypeName().CamelCaseToSpaced());
                    break;
            }
        }

        private void SetColor(Color color)
        {
            nodeView.Find("node-type-color-top").ChangeBackgroundColor(color);
            nodeView.Find("node-type-color-bottom").ChangeBackgroundColor(color);
        }

        private void SetIconText(string text)
        {
            var iconElement = nodeView.Find("node-icon");
            iconElement.Enable();
            iconElement.Add(new Label(text));
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
                InputPort = nodeView.InstantiatePort(Direction.Input, EPortOrientation.Bottom);
            }
            else nodeView.inputContainer.Disable();

            if (node != null || node.MaxOutputConnections != 0)
            {
                OutputPort = nodeView.InstantiatePort(Direction.Output, EPortOrientation.Top);
            }
            else nodeView.outputContainer.Disable();

        }

        public override PortView GetPort(MNodeView nodeView, Direction direction)
        {
            if(direction == Direction.Input) return InputPort;
            else return OutputPort;
        }
    }
}

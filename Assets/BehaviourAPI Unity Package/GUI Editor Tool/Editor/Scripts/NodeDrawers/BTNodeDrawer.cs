using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    [CustomNodeDrawer(typeof(BTNode))]
    public class BTNodeDrawer : NodeDrawer
    {
        PortView InputPort, OutputPort;

        VisualElement rootIcon;
        public override string LayoutPath => BehaviourAPISettings.instance.EditorLayoutsPath + "Nodes/Tree Node.uxml";

        public override void DrawNodeDetails()
        {
            rootIcon = view.Q("node-root");
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

        public override void OnRepaint()
        {
            if (view.graphView.graphData.nodes.First() == view.data && IsValidRootNode(view.data))
            {
                if(view.data.parentIds.Count > 0)
                {

                }
                else
                {
                    view.inputContainer.Hide();
                    rootIcon.Enable();
                }
            }
            else
            {
                view.inputContainer.Show();
                rootIcon.Disable();
            }
        }

        public override void SetUpPorts()
        {
            if (node == null || node.MaxInputConnections != 0)
            {
                InputPort = view.InstantiatePort(Direction.Input, EPortOrientation.Bottom);
            }
            else view.inputContainer.Disable();

            if (node == null || node.MaxOutputConnections != 0)
            {
                OutputPort = view.InstantiatePort(Direction.Output, EPortOrientation.Top);
            }
            else view.outputContainer.Disable();

        }

        public override PortView GetPort(MNodeView nodeView, Direction direction)
        {
            if(direction == Direction.Input) return InputPort;
            else return OutputPort;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (view.graphView.IsRuntime) return;

            evt.menu.AppendAction("Convert to root node",
                _ => ConvertToRootNode(),
                (view.GetDataIndex() != 0) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Order childs by x position", _ => view.OrderChildNodes(n => n.position.x),
                view.data.childIds.Count > 1 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
        }

        private void ConvertToRootNode()
        {
            view.DisconnectAllInputPorts();
            view.ConvertToFirstNode();
        }

        public override void OnDeleted()
        {
            RecomputeRootNode();
        }

        private static bool IsValidRootNode(NodeData data)
        {
            return data.parentIds.Count == 0;
        }

        private void RecomputeRootNode()
        {
            var nodes = view.graphView.graphData.nodes;
            if (nodes.Count > 0 && !IsValidRootNode(nodes.First()))
            {
                var newRootNode = nodes.FirstOrDefault(IsValidRootNode);

                if (newRootNode != null) nodes.MoveAtFirst(newRootNode);
            }
        }
    }
}

using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public class DefaultNodeView : NodeView
    {
        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public DefaultNodeView(NodeAsset node) : base(node, NODE_LAYOUT)
        {

        }

        protected override void DrawPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
            {
                var capacity = Node.Node.MaxInputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Input, capacity, Node.Node.GetType());
                port.portName = "";
                port.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(port);
                InputPorts.Add(port);
            }

            if (Node.Node.MaxOutputConnections != 0)
            {
                var capacity = Node.Node.MaxOutputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, Node.Node.ChildType);
                port.portName = "";
                port.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(port);
                OutputPorts.Add(port);
            }
        }

        protected override void DrawExtensionContainer()
        {
            var extensionContainer = this.Q(name: "extension");

            if (Node.Node is IActionHandler actionHandler)
            {
                var containerView = new ContainerView(Node);
                extensionContainer.Add(containerView);
            }
        }

        protected override void AddLayout()
        {
           
        }
    }
}

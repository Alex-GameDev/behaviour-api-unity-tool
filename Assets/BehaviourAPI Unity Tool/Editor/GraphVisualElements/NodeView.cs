namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
    using BehaviourAPI.Unity.Runtime;
    using Core;
    using System;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using UnityEngine;
    using Vector2 = UnityEngine.Vector2;
    using Action = Core.Actions.Action;
    using System.Linq;

    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeAsset Node { get; set; }

        public Action<NodeAsset> Selected = delegate { };

        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public NodeView(NodeAsset node) : base(NODE_LAYOUT)
        {
            Node = node;
            SetPosition(new UnityEngine.Rect(node.Position, Vector2.zero));
            DrawPorts();
            DrawExtensionContainer();
            styleSheets.Add(VisualSettings.GetOrCreateSettings().NodeStylesheet);
            SetUpContextualMenu();
            SetUpDataBinding();
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Test", dd => TestAction());
            }));
        }

        private void TestAction()
        {
           
        }

        void DrawPorts()
        {
            if (Node.Node.MaxInputConnections != 0)
            {
                var capacity = Node.Node.MaxInputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Input, capacity, Node.Node.GetType());
                port.portName = "";
                port.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(port);
            }

            if (Node.Node.MaxOutputConnections != 0)
            {
                var capacity = Node.Node.MaxOutputConnections == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                var port = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, Node.Node.ChildType);
                port.portName = "";
                port.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(port);
            }
        }

        /// <summary>
        /// Draws the visual elements to assign actions and perceptions
        /// </summary>
        void DrawExtensionContainer()
        {
            var extensionContainer = this.Q(name: "extension");

            SerializedObject nodeObj = new SerializedObject(Node);
            var prop = nodeObj.GetIterator();
            bool propertiesLeft = true;
            while (propertiesLeft)
            {
                if (prop.propertyType == SerializedPropertyType.ManagedReference)
                {
                    var typeName = prop.managedReferenceFieldTypename.Split(' ').Last();

                    if (typeName == typeof(Action).FullName)
                    {
                        var containerView = new ContainerView(Node, prop.Copy());
                        extensionContainer.Add(containerView);
                    }

                    if (typeName == typeof(Perception).FullName)
                    {
                        var containerView = new ContainerView(Node, prop.Copy());
                        extensionContainer.Add(containerView);
                    }
                }
                propertiesLeft = prop.NextVisible(true);
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            Selected?.Invoke(Node);
        }

        public void OnConnected(Direction direction, NodeView other)
        {
            if(direction == Direction.Input)
                Node.Parents.Add(other.Node);
            else
                Node.Childs.Add(other.Node);
        }

        public void OnMoved(Vector2 pos)
        {
            Node.Position = pos;
        }

        public void OnDisconnected(Direction direction, NodeView other)
        {
            if (direction == Direction.Input)
                Node.Parents.Remove(other.Node);
            else
                Node.Childs.Remove(other.Node);           
        }

        void SetUpDataBinding()
        {
            var titleInputField = this.Q<TextField>(name: "title-input-field");
            titleInputField.bindingPath = "Name";
            titleInputField.Bind(new SerializedObject(Node));
        }
    }
}
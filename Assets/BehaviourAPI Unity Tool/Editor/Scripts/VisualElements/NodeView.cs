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
    using System.Collections.Generic;
    using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;

    /// <summary>
    /// Visual element that represents a node in a behaviour graph
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeAsset Node { get; set; }

        public Action<NodeAsset> Selected = delegate { };

        BehaviourGraphView _graphView;
        public BehaviourGraphView GraphView => _graphView;

        VisualElement _rootElement;

        public static string NODE_LAYOUT => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        public NodeView(NodeAsset node, BehaviourGraphView graphView) : base(NODE_LAYOUT)
        {
            Node = node;
            _graphView = graphView;
            _rootElement = this.Q("node-root");
            SetPosition(new Rect(node.Position, Vector2.zero));
            DrawPorts();
            DrawExtensionContainer();
            styleSheets.Add(VisualSettings.GetOrCreateSettings().NodeStylesheet);
            SetUpContextualMenu();
            SetUpDataBinding();

            if (graphView.Runtime) AddRuntimeLayout();
        }

        private void AddRuntimeLayout()
        {
            if(Node.Node is IStatusHandler statusHandler)
            {
                var statusBorder = this.Q("node-status");
                statusHandler.StatusChanged += status => UpdateStatusBorder(statusBorder, status);

                UpdateStatusBorder(statusBorder, statusHandler.Status);              
            }
        }

        void UpdateStatusBorder(VisualElement statusBorder, Status status)
        {
            if(status != Status.None)
            {
                statusBorder.style.borderBottomColor = StatusToColor(status);
                statusBorder.style.borderTopColor = StatusToColor(status);
                statusBorder.style.borderLeftColor = StatusToColor(status);
                statusBorder.style.borderRightColor = StatusToColor(status);
            }
        }

        Color StatusToColor(Status status)
        {
            if (status == Status.Success) return Color.green;
            if (status == Status.Failure) return Color.red;
            if (status == Status.Running) return Color.yellow;
            return Color.gray;
        }

        private void SetUpContextualMenu()
        {
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("SetAsRootNode", _ => { DisconnectPorts(inputContainer); _graphView.SetRootNode(this); }, _ => DropdownMenuAction.Status.Normal);
                menuEvt.menu.AppendAction("Disconnect input ports", _ => DisconnectPorts(inputContainer), _ => Node.Node.MaxInputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Disconnect output ports", _ => DisconnectPorts(outputContainer), _ => Node.Node.MaxOutputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
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
                        var containerView = new ActionContainerView(Node, prop.Copy(), this);
                        extensionContainer.Add(containerView);
                    }

                    if (typeName == typeof(Perception).FullName)
                    {
                        var containerView = new PerceptionContainerView(Node, prop.Copy(), this);
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

        public void SetAsStartNode()
        {
            inputContainer.style.display = DisplayStyle.None;
            _rootElement.style.display = DisplayStyle.Flex;
        }

        public void QuitAsStartNode()
        {
            inputContainer.style.display = DisplayStyle.Flex;
            _rootElement.style.display = DisplayStyle.None;
        }


        void DisconnectPorts(VisualElement portContainer)
        {
            if(GraphView != null)
            {
                var elements = new List<GraphElement>();
                portContainer.Query<Port>().ForEach(port =>
                {
                    if (port.connected)
                    {
                        foreach (var c in port.connections) elements.Add(c);
                    }
                });
                GraphView.DeleteElements(elements);
            }
        }
    }
}
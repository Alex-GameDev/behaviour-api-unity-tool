namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Perceptions;
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
    using BehaviourAPI.Unity.Framework;
    using BehaviourAPI.UnityExtensions;


    /// <summary>
    /// Visual element that represents a data in a behaviour graph
    /// </summary>
    public abstract class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        #region --------------------------- Fields ---------------------------
        
        public NodeData Node;

        public Action<NodeData> Selected;
        public Action<NodeData> UnSelected;

        protected BehaviourGraphView _graphView;
        public BehaviourGraphView GraphView => _graphView;

        List<PortView> inputPorts, outputPorts;

        protected List<EdgeView> outputEdges = new List<EdgeView>();

        #endregion

        #region ----------------------- Visual elements -----------------------
        public VisualElement RootElement { get; private set; }
        public VisualElement IconElement { get; private set; }
        public VisualElement BorderElement { get; private set; }

        public PortView InputPort => InputPorts.FirstOrDefault();
        public PortView OutputPort => OutputPorts.FirstOrDefault();

        public List<PortView> InputPorts => inputPorts;
        public List<PortView> OutputPorts => outputPorts;


        #endregion

        public abstract string LayoutPath { get; }     

        #region --------------------------- Set up ---------------------------
        public NodeView(NodeData node, BehaviourGraphView graphView, string path) : base(path)
        {
            Node = node;
            _graphView = graphView;
            inputPorts = new List<PortView>();
            outputPorts = new List<PortView>();
            RootElement = this.Q("node-root");
            IconElement = this.Q("node-icon");
            BorderElement = this.Q("node-border");

            SetPosition(new Rect(node.Position, Vector2.zero));
            SetUpPorts();
            SetUpDataBinding();

            if(Node.Node != null)
            {
                if(graphView.Runtime)
                {
                    AddRuntimeLayout();
                }
                else
                {
                    DrawExtensionContainer();
                    SetUpContextualMenu();
                }
            }
            else
            {
                IconElement.Add(new Label("Missing node"));
                IconElement.Enable();
            }            
        }

        protected virtual void AddRuntimeLayout()
        {
            this.Q("node-port-cover").Enable();
            if(Node.Node is IStatusHandler statusHandler)
            {
                var statusBorder = this.Q("node-status");
                statusHandler.StatusChanged += status => statusBorder.ChangeBorderColor(status.ToColor());

                statusBorder.ChangeBorderColor(statusHandler.Status.ToColor());
            }
        }

        private void SetUpContextualMenu()
        {           
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Duplicate node", _ => _graphView.DuplicateNode(Node));
                menuEvt.menu.AppendAction("Disconnect input ports", _ => DisconnectPorts(inputContainer), _ => Node.Node.MaxInputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Disconnect output ports", _ => DisconnectPorts(outputContainer), _ => Node.Node.MaxOutputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
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
                // Actions
                if (prop.propertyType == SerializedPropertyType.ManagedReference)
                {
                    var typeName = prop.managedReferenceFieldTypename.Split(' ').Last();

                    if (typeName == typeof(Action).FullName)
                    {
                        var containerView = new ActionContainerView(Node, prop.Copy(), this);
                        extensionContainer.Add(containerView);
                    }                   
                }
                // Perceptions
                else if(prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (prop.displayName == "Perception")
                    {
                        var containerView = new PerceptionContainerView(Node, prop.Copy(), this);
                        extensionContainer.Add(containerView);
                    }
                }
                propertiesLeft = prop.NextVisible(true);
            }
        }

        void SetUpDataBinding()
        {
            var obj = new SerializedObject(Node);
            var titleInputField = this.Q<TextField>(name: "title-input-field");

            if (BehaviourEditorWindow.Instance.IsRuntime)
            {
                titleInputField.isReadOnly = true;
            }

            titleInputField.bindingPath = "Name";
            titleInputField.Bind(obj);
        }

        protected PortView InstantiatePort(Direction direction, PortOrientation orientation)
        {
            var isInput = direction == Direction.Input;

            Port.Capacity portCapacity;
            Type portType;

            if(Node.Node != null)
            {
                if (isInput) portCapacity = Node.Node.MaxInputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                else portCapacity = Node.Node.MaxOutputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                portType = isInput ? Node.Node.GetType() : Node.Node.ChildType;
            }
            else
            {
                portCapacity = Port.Capacity.Multi;
                portType = typeof(NodeView); // Any invalid type
            }

            var port = PortView.Create(orientation, direction, portCapacity, portType);

            (isInput ? InputPorts : OutputPorts).Add(port);
            (isInput ? inputContainer : outputContainer).Add(port);

            port.portName = "";
            port.style.flexDirection = orientation.ToFlexDirection();

            var bg = new VisualElement();
            bg.style.position = Position.Absolute;
            bg.style.top = 0; bg.style.left = 0; bg.style.bottom = 0; bg.style.right = 0;
            port.Add(bg);

            return port;
        }

        public void ChangeTypeColor(Color color)
        {
            this.Q("node-type-color-top").ChangeBackgroundColor(color);
            this.Q("node-type-color-bottom").ChangeBackgroundColor(color);
        }

        #endregion

        #region --------------------------- Editor events ---------------------------

        public override void OnSelected()
        {
            base.OnSelected();
            BorderElement.ChangeBackgroundColor(new Color(.5f, .5f, .5f, .5f));
            Selected?.Invoke(Node);
        }

        public override void OnUnselected()
        {
            BorderElement.ChangeBackgroundColor(new Color(0f, 0f, 0f, 0f));
            base.OnUnselected();
            Selected?.Invoke(null);
        }

        public virtual void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    Node.Parents.Add(other.Node);
                }
                else
                {
                    Node.Childs.Add(other.Node);
                }
            }

            if (port.direction == Direction.Output)
            {
                outputEdges.Add(edgeView);
                UpdateEdgeViews();
            }

        }

        public void OnMoved(Vector2 pos)
        {
           Node.Position = pos;
        }

        public virtual void OnDisconnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    Node.Parents.Remove(other.Node);
                }
                else
                {
                    Node.Childs.Remove(other.Node);
                }
            }

            if (port.direction == Direction.Output)
            {
                outputEdges.Remove(edgeView);
                UpdateEdgeViews();
            }
        }

        public void DisconnectPorts(VisualElement portContainer)
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

        #endregion 

        public abstract void SetUpPorts();

        public virtual Port GetBestPort(NodeView other, Direction dir)
        {
            if (dir == Direction.Input) return InputPort;
            else return OutputPort;
        }

        public void UpdateEdgeViews()
        {
            var childs = Node.Childs;

            if(childs.Count <= 1)
            {
                foreach (var edgeView in outputEdges)
                {
                    edgeView.control.UpdateIndex(0);
                }
            }
            else
            {
                foreach (var edgeView in outputEdges)
                {
                    var target = (edgeView.input.node as NodeView).Node;
                    int idx = childs.IndexOf(target);
                    edgeView.control.UpdateIndex(idx + 1);
                }
            }
        }

        public void OrderChilds(Func<NodeData, float> sortFunction, bool notify = true)
        {
            Node.OrderChilds(sortFunction);
            UpdateEdgeViews();
            if(notify) BehaviourEditorWindow.Instance.OnModifyAsset();
        }
    }
}
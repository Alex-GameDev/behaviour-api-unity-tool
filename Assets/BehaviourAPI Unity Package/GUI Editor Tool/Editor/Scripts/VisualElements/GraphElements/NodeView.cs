using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using Core;
    using Action = Core.Actions.Action;
    using Vector2 = Vector2;

    using Framework;
    using UnityExtensions;
    using BehaviourAPI.Unity.Framework.Adaptations;
    using BehaviourAPI.Core.Perceptions;

    /// <summary>
    /// Visual element that represents a data in a behaviour graph
    /// </summary>
    public abstract class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public abstract string LayoutPath { get; }

        #region --------------------------- Fields ---------------------------

        public NodeData Node { get; private set; }
        public BehaviourGraphView GraphView { get; private set; }

        public System.Action Selected, Unselected;

        SerializedProperty _property;
        #endregion

        #region ----------------------- Visual elements -----------------------
        public VisualElement RootElement { get; private set; }
        public VisualElement IconElement { get; private set; }
        public VisualElement BorderElement { get; private set; }
        public VisualElement TypeColorTop { get; private set; }
        public VisualElement TypeColorBottom { get; private set; }

        public List<PortView> InputPorts { get; } = new List<PortView>();
        public List<PortView> OutputPorts { get; } = new List<PortView>();

        protected List<EdgeView> outputEdges = new List<EdgeView>();

        TextField _titleInputField;

        #endregion     

        #region --------------------------- Set up ---------------------------
        public NodeView(NodeData node, BehaviourGraphView graphView, string path) : base(path)
        {
            Node = node;
            GraphView = graphView;

            RefreshProperty();

            RootElement = this.Q("node-root");
            IconElement = this.Q("node-icon");
            BorderElement = this.Q("node-border");

            _titleInputField = this.Q<TextField>(name: "title-input-field");

            SetPosition(new Rect(node.position, Vector2.zero));

            SetUpPorts();

            if(Node.node != null)
            {
                if(graphView.Runtime)
                {
                    _titleInputField.value = node.name;
                    _titleInputField.isReadOnly = true;
                    AddRuntimeLayout();
                }
                else
                {
                    SetUpDataBinding();
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
            if(Node.node is IStatusHandler statusHandler)
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
                menuEvt.menu.AppendAction("Duplicate node", _ => GraphView.DuplicateNode(Node));
                menuEvt.menu.AppendAction("Disconnect input ports", _ => DisconnectPorts(inputContainer), _ => Node.node.MaxInputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                menuEvt.menu.AppendAction("Disconnect output ports", _ => DisconnectPorts(outputContainer), _ => Node.node.MaxOutputConnections != 0 ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }));
        }

        /// <summary>
        /// Draws the visual elements to display actions and perceptions
        /// </summary>
        void DrawExtensionContainer()
        {
            var extensionContainer = this.Q(name: "extension");

            if(Node.node is IActionAssignable actionAssignable)
            {
                var label = new Label($"{GetActionInfo(actionAssignable.ActionReference)}");
                label.AddToClassList("node-text");
                extensionContainer.Add(label);
            }

            if(Node.node is IPerceptionAssignable perceptionAssignable)
            {
                 var label = new Label($"if {GetPerceptionInfo(perceptionAssignable.PerceptionReference)}");
                label.AddToClassList("node-text");
                extensionContainer.Add(label);
            }
        }

        string GetActionInfo(Action action)
        {
            switch(action)
            {
                case CustomAction:
                    return "Custom Action";
                case UnityAction unityAction:
                    return unityAction.DisplayInfo;
                case SubgraphAction subgraphAction:
                    if(string.IsNullOrEmpty(subgraphAction.subgraphId))
                    {
                        return "Subgraph: Unasigned";
                    }
                    else
                    {
                        var graph = BehaviourEditorWindow.Instance.System.Data.graphs.Find(g => g.id == subgraphAction.subgraphId);
                        if(graph == null) return "Subgraph: missing subgraph";
                        else return "Subgraph: " + graph.name;
                    }                   
                    
                default:
                    return "(No action)";
            }
        }

        string GetPerceptionInfo(Perception perception)
        {
            switch (perception)
            {
                case CustomPerception customPerception:
                    return "Custom Perception";
                case UnityPerception unityPerception:
                    return unityPerception.DisplayInfo;
                case CompoundPerceptionWrapper compoundPerception:
                    var compoundType = compoundPerception.compoundPerception.GetType();
                    var logicCharacter = compoundType == typeof(AndPerception) ? " && " :
                        compoundType == typeof(OrPerception) ? " || " : " - ";
                    return "(" + compoundPerception.subPerceptions.Select(sub => GetPerceptionInfo(sub.perception)).Join(logicCharacter) +")";
                default:
                    return "(No perception)";
            }
        }

        void SetUpDataBinding()
        {
            if(_titleInputField == null) _titleInputField = this.Q<TextField>(name: "title-input-field");
            _titleInputField.bindingPath = _property.propertyPath + ".name";
            _titleInputField.Bind(_property.serializedObject);
        }

        public void RefreshProperty()
        {
            if (BehaviourEditorWindow.Instance.IsRuntime) return;

            _property = GetPropertyPath();
            SetUpDataBinding();
        }

        SerializedProperty GetPropertyPath()
        {
            var graphDataId = BehaviourEditorWindow.Instance.System.Data.graphs.IndexOf(GraphView.graphData);
            var nodeDataId = GraphView.graphData.nodes.IndexOf(Node);

            var path = $"data.graphs.Array.data[{graphDataId}].nodes.Array.data[{nodeDataId}]";
            var prop = new SerializedObject(BehaviourEditorWindow.Instance.System.ObjectReference).FindProperty(path);
            return prop;
        }

        protected PortView InstantiatePort(Direction direction, PortOrientation orientation)
        {
            var isInput = direction == Direction.Input;

            Port.Capacity portCapacity;
            Type portType;

            if(Node.node != null)
            {
                if (isInput) portCapacity = Node.node.MaxInputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                else portCapacity = Node.node.MaxOutputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                portType = isInput ? Node.node.GetType() : Node.node.ChildType;
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
            Selected?.Invoke();
        }

        public override void OnUnselected()
        {
            BorderElement.ChangeBackgroundColor(new Color(0f, 0f, 0f, 0f));
            base.OnUnselected();
            Unselected?.Invoke();
        }

        public virtual void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    Node.parentIds.Add(other.Node.id);
                }
                else
                {
                    Node.childIds.Add(other.Node.id);
                }
            }

            if (port.direction == Direction.Output)
            {
                outputEdges.Add(edgeView);
                UpdateEdgeViews();
            }
        }

        public virtual void OnDisconnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    Node.parentIds.Remove(other.Node.id);
                }
                else
                {
                    Node.childIds.Remove(other.Node.id);
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
            if (dir == Direction.Input) return InputPorts.FirstOrDefault();
            else return OutputPorts.FirstOrDefault();
        }

        public void UpdateEdgeViews()
        {
            var childs = Node.childIds;

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
                    int idx = childs.IndexOf(target.id);
                    edgeView.control.UpdateIndex(idx + 1);
                }
            }
        }
    }
}
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Text;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core;
    using BehaviourAPI.UnityExtensions;
    using Framework;
    using Framework.Adaptations;
    using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer.Explorer;

    /// <summary>
    /// 
    /// </summary>
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeData data;
        
        NodeDrawer drawer;

        SerializedProperty nodeProperty;

        public List<EdgeView> InputConnectionViews { get; private set; } = new List<EdgeView>();
        public List<EdgeView> OutputConnectionViews { get; private set; } = new List<EdgeView>();

        public VisualElement BorderElement { get; private set; }
        public TaskView ActionView { get; private set; }
        public TaskView PerceptionView { get; private set; }
        public TaskView CustomView { get; private set; }

        VisualElement m_StatusBorder;

        TextField m_TitleInputField;

        public BehaviourGraphView graphView
        {
            get
            {
                if (m_graphView == null) m_graphView = GetFirstAncestorOfType<BehaviourGraphView>();
                return m_graphView;
            }
        }

        private BehaviourGraphView m_graphView;
        IEdgeConnectorListener edgeConnector;

        
        public NodeView(NodeData data, NodeDrawer drawer, BehaviourGraphView graphView, IEdgeConnectorListener edgeConnector, SerializedProperty property = null) : base(drawer.LayoutPath)
        {
            this.data = data;
            this.drawer = drawer;

            m_TitleInputField = this.Q<TextField>(name: "title-input-field");

            drawer.SetView(this, data.node);
            this.m_graphView = graphView;
            SetPosition(new Rect(data.position, UnityEngine.Vector2.zero));
            this.edgeConnector = edgeConnector;

            BorderElement = this.Q("node-border");

            var extensionContainer = this.Q("node-extension-content");
            ActionView = new TaskView();
            extensionContainer.Add(ActionView);
            PerceptionView = new TaskView();
            extensionContainer.Add(PerceptionView);
            CustomView = new TaskView();
            extensionContainer.Add(CustomView);

            UpdateSerializedProperty(property);
            drawer.SetUpPorts();
            drawer.DrawNodeDetails();

            m_StatusBorder = this.Q("node-status");
            if (graphView.IsRuntime)
            {
                this.Q("node-port-cover").Enable();
                if(data.node is IStatusHandler statusHandler)
                {
                    statusHandler.StatusChanged += OnStatusChanged;
                    OnStatusChanged(statusHandler.Status);
                }
            }
            RefreshDisplay();
        }

        public void OnDestroy()
        {
            if (graphView.IsRuntime && data.node is IStatusHandler statusHandler)
            {
                statusHandler.StatusChanged -= OnStatusChanged;
            }
            drawer.OnDestroy();
        }

        private void OnStatusChanged(Status status)
        {
            m_StatusBorder.ChangeBorderColor(status.ToColor());
        }

        public void UpdateSerializedProperty(SerializedProperty prop)
        {
            nodeProperty = prop;
            UpdatePropertyBinding();
        }

        private void UpdatePropertyBinding()
        {
            m_TitleInputField.value = data.name;
            if (nodeProperty == null)
            {
                m_TitleInputField.isReadOnly = true;
            }
            else
            {
                // The binding path needs to be assigned before the object:
                m_TitleInputField.bindingPath = nodeProperty.FindPropertyRelative("name").propertyPath;
                m_TitleInputField.Bind(nodeProperty.serializedObject);
                m_TitleInputField.isReadOnly = false;
            }
        }

        public void RefreshDisplay()
        {
            //var t = DateTime.Now;
            if (data.node is IActionAssignable actionAssignable) 
            {
                ActionView.Update(actionAssignable.ActionReference.GetActionInfo());
            }
            if (data.node is IPerceptionAssignable perceptionAssignable)
            {
                var perceptionInfo = perceptionAssignable.PerceptionReference.GetPerceptionInfo();
                if (string.IsNullOrWhiteSpace(perceptionInfo)) perceptionInfo = "true";
                PerceptionView.Update("if " + perceptionInfo);
            }
            drawer.OnRefreshDisplay();

            if(ActionView.Container.style.display == DisplayStyle.None &&
                PerceptionView.Container.style.display == DisplayStyle.None &&
                CustomView.Container.style.display == DisplayStyle.None)
            {
                var toggle = this.Q<Foldout>("node-extension-foldout");
                toggle.Disable();
            }
            else
            {
                var toggle = this.Q<Foldout>("node-extension-foldout");
                toggle.Enable();
            }
            //Debug.Log((DateTime.Now - t).TotalMilliseconds);
        }


        #region ------------------------------- Events -------------------------------

        public override void OnSelected()
        {
            base.OnSelected();
            BorderElement.ChangeBackgroundColor(new Color(.5f, .5f, .5f, .5f));
            drawer.OnSelected();
        }

        public override void OnUnselected()
        {
            base.OnSelected();
            BorderElement.ChangeBackgroundColor(new Color(0f, 0f, 0f, 0f));
            drawer.OnUnselected();
        }

        /// <summary>
        /// Call this when the node is moved
        /// </summary>
        public void OnMoved()
        {
            data.position = GetPosition().position;
            drawer.OnMoved();
        }

        public void OnConnected(EdgeView edgeView, bool updateData = true)
        {
            if (edgeView.output.node == this)
            {
                if (updateData)
                {
                    var other = edgeView.input.node as NodeView;
                    data.childIds.Add(other.data.id);
                }
                OutputConnectionViews.Add(edgeView);
                UpdateChildConnectionViews();
            }
            else
            {
                if (updateData)
                {
                    var other = edgeView.output.node as NodeView;
                    data.parentIds.Add(other.data.id);
                }
                InputConnectionViews.Add(edgeView);
                UpdateParentConnectionViews();
            }
            drawer.OnConnected(edgeView);
        }

        /// <summary>
        /// Call when an edge is disconnected to the node.
        /// </summary>

        public void OnDisconnected(EdgeView edgeView, bool updateData = true)
        {
            if (edgeView.output.node == this)
            {
                if (updateData)
                {
                    var other = edgeView.input.node as NodeView;
                    data.childIds.Remove(other.data.id);
                }
                OutputConnectionViews.Remove(edgeView);
                UpdateChildConnectionViews();
            }
            else
            {
                if (updateData)
                {
                    var other = edgeView.output.node as NodeView;
                    data.parentIds.Remove(other.data.id);
                }
                InputConnectionViews.Remove(edgeView);
                UpdateParentConnectionViews();
            }
            drawer.OnDisconnected(edgeView);
        }

        #endregion


        public PortView InstantiatePort(Direction direction, EPortOrientation orientation)
        {
            var isInput = direction == Direction.Input;

            Port.Capacity portCapacity;
            Type portType;

            if (data.node != null)
            {
                if (isInput) portCapacity = data.node.MaxInputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                else portCapacity = data.node.MaxOutputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                portType = isInput ? data.node.GetType() : data.node.ChildType;
            }
            else
            {
                portCapacity = Port.Capacity.Multi;
                portType = typeof(NodeView); // Any invalid type
            }

            var port = PortView.Create(orientation, direction, portCapacity, portType, edgeConnector);
            (isInput ? inputContainer : outputContainer).Add(port);

            if (graphView.IsRuntime)
            {
                port.capabilities -= Capabilities.Selectable;
            }

            port.portName = "";
            port.style.flexDirection = orientation.ToFlexDirection();

            var bg = new VisualElement();
            bg.style.position = Position.Absolute;
            bg.style.top = 0; bg.style.left = 0; bg.style.bottom = 0; bg.style.right = 0;
            port.Add(bg);

            return port;
        }

        #region ------------------------------- Contextual menu -------------------------------

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if(!graphView.IsRuntime)
            {
                evt.menu.AppendAction("Disconnect all.", _ => DisconnectAll(),
                (InputConnectionViews.Count > 0 || OutputConnectionViews.Count > 0) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction("Disconnect all input edges.", _ => DisconnectAllInput(),
                    (InputConnectionViews.Count > 0) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendAction("Disconnect all output edges.", _ => DisconnectAllOutput(),
                    (OutputConnectionViews.Count > 0) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
            }

            evt.menu.AppendAction("Debug", _ => DebugNode());

            drawer.BuildContextualMenu(evt);
            evt.StopPropagation();
        }

        public void OrderChildNodes(Func<NodeData, float> orderFunction)
        {
            m_graphView.RegisterUndo("Order child nodes");
            graphView.graphData.OrderChildNodes(data, (n) => n.position.x);
            UpdateChildConnectionViews();
            graphView.UpdateProperties();
        }

        private void DisconnectAll()
        {
            DisconnectAllInput();
            DisconnectAllOutput();
        }

        private void DisconnectAllInput()
        {
            graphView.DeleteElements(InputConnectionViews);
            InputConnectionViews.Clear();
        }

        private void DisconnectAllOutput()
        {
            graphView.DeleteElements(OutputConnectionViews);
            OutputConnectionViews.Clear();
        }

        private void DebugNode()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name: " + data.name);
            sb.AppendLine("Id: " + data.id);
            sb.AppendLine("Parents (" + data.parentIds.Count + ") :");

            for(int i = 0; i < data.parentIds.Count; i++)
            {
                sb.AppendLine("\t- " + data.parentIds[i]);
            }

            sb.AppendLine("Children (" + data.childIds.Count + ") :");

            for (int i = 0; i < data.childIds.Count; i++)
            {
                sb.AppendLine("\t- " + data.childIds[i]);
            }

            sb.AppendLine("Index: " + graphView.graphData.nodes.IndexOf(data).ToString());
            Debug.Log(sb.ToString());
        }

        #endregion

        public Port GetBestPort(NodeView targetNodeView, Direction direction) => drawer.GetPort(targetNodeView, direction);

        public VisualElement Find(string name) => this.Q(name);

        public int GetDataIndex()
        {
            var graphData = graphView.graphData;
            return graphData.nodes.IndexOf(data);
        }

        public void ConvertToFirstNode()
        {
            graphView.SetNodeAsFirst(this);
        }

        public void DisconnectAllInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        public void DisconnectPorts(VisualElement portContainer)
        {
            if (graphView != null)
            {
                var elements = new List<GraphElement>();
                portContainer.Query<Port>().ForEach(port =>
                {
                    if (port.connected)
                    {
                        foreach (var c in port.connections) elements.Add(c);
                    }
                });
                graphView.DeleteElements(elements);
            }
        }

        public void RefreshView()
        {
            SetPosition(new Rect(data.position, Vector2.zero));
            drawer.OnRepaint();
        }

        internal void OnDeleted()
        {
            drawer.OnDeleted();
        }

        private void UpdateChildConnectionViews()
        {
            var childs = data.childIds;
            if (childs.Count <= 1)
            {
                foreach (var edgeView in OutputConnectionViews)
                {
                    edgeView.control.UpdateIndex(0);
                }
            }
            else
            {
                foreach (var edgeView in OutputConnectionViews)
                {
                    var target = (edgeView.input.node as NodeView).data;
                    int idx = childs.IndexOf(target.id);
                    edgeView.control.UpdateIndex(idx + 1);
                }
            }
        }

        private void UpdateParentConnectionViews()
        {

        }
    }

    public class TaskView : VisualElement
    {
        public Label Label { get; private set; }
        public VisualElement Container { get; private set; }

        public TaskView()
        {
            var asset = BehaviourAPISettings.instance.GetLayoutAsset("Elements/taskview.uxml");
            asset.CloneTree(this);

            Label = this.Q<Label>("tv-label");
            Container = this.Q("tv-container");
            Container.Disable();
        }

        public void Update(string text)
        {            
            if(string.IsNullOrEmpty(text))
            {
                Container.Disable();
            }
            else
            {
                Container.Enable();
            }
            Label.text = text;
        }

        public void AddElement(VisualElement visualElement)
        {
            Container.Add(visualElement);
        }
    }
}
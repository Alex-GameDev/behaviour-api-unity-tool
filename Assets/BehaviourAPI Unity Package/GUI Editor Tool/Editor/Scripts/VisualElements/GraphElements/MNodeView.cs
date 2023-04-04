using BehaviourAPI.Unity.Framework;
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
    /// <summary>
    /// 
    /// </summary>
    public class MNodeView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeData data;
        NodeDrawer drawer;

        SerializedProperty nodeProperty;

        private List<EdgeView> InputConnectionViews = new List<EdgeView>();
        private List<EdgeView> OutputConnectionViews = new List<EdgeView>();


        public VisualElement BorderElement { get; private set; }

        TextField m_TitleInputField;

        public GraphDataView graphView
        {
            get
            {
                if (m_graphView == null) m_graphView = GetFirstAncestorOfType<GraphDataView>();
                return m_graphView;
            }
        }

        private GraphDataView m_graphView;
        IEdgeConnectorListener edgeConnector;

        public MNodeView(NodeData data, NodeDrawer drawer, GraphDataView graphView, IEdgeConnectorListener edgeConnector, SerializedProperty property = null) : base(drawer.LayoutPath)
        {
            this.data = data;
            this.drawer = drawer;

            m_TitleInputField = this.Q<TextField>(name: "title-input-field");

            drawer.SetView(this, data.node);
            this.m_graphView = graphView;
            SetPosition(new Rect(data.position, Vector2.zero));
            this.edgeConnector = edgeConnector;

            BorderElement = this.Q("node-border");


            UpdateSerializedProperty(property);
            drawer.SetUpPorts();
            drawer.DrawNodeDetails();
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
        }

        public void OnConnected(EdgeView edgeView, bool updateData = true)
        {
            if (edgeView.output.node == this)
            {
                if (updateData)
                {
                    var other = edgeView.input.node as MNodeView;
                    data.childIds.Add(other.data.id);
                }
                OutputConnectionViews.Add(edgeView);
                UpdateChildConnectionViews();
            }
            else
            {
                if (updateData)
                {
                    var other = edgeView.output.node as MNodeView;
                    data.parentIds.Add(other.data.id);
                }
                InputConnectionViews.Add(edgeView);
                UpdateParentConnectionViews();
            }
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
                    var other = edgeView.input.node as MNodeView;
                    data.childIds.Remove(other.data.id);
                }
                OutputConnectionViews.Remove(edgeView);
                UpdateChildConnectionViews();
            }
            else
            {
                if (updateData)
                {
                    var other = edgeView.output.node as MNodeView;
                    data.parentIds.Remove(other.data.id);
                }
                InputConnectionViews.Remove(edgeView);
                UpdateParentConnectionViews();
            }          
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
                portType = typeof(MNodeView); // Any invalid type
            }

            var port = PortView.Create(orientation, direction, portCapacity, portType, edgeConnector);
            (isInput ? inputContainer : outputContainer).Add(port);

            port.portName = "";
            port.style.flexDirection = orientation.ToFlexDirection();

            var bg = new VisualElement();
            bg.style.position = Position.Absolute;
            bg.style.top = 0; bg.style.left = 0; bg.style.bottom = 0; bg.style.right = 0;
            port.Add(bg);

            return port;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Debug", _ => DebugNode());
            drawer.BuildContextualMenu(evt);
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

            sb.AppendLine(graphView.graphData.nodes.IndexOf(data).ToString());
            sb.AppendLine(nodeProperty.propertyPath);
            Debug.Log(sb.ToString());
        }

        public Port GetBestPort(MNodeView targetNodeView, Direction direction) => drawer.GetPort(targetNodeView, direction);

        public T Find<T>(string name) where T : VisualElement => this.Q<T>(name);

        public VisualElement Find(string name) => this.Q(name);

        public int GetDataIndex()
        {
            var graphData = graphView.graphData;
            return graphData.nodes.IndexOf(data);
        }

        public void ConvertToFirstNode()
        {
            graphView.ReorderNode(this);
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
                    var target = (edgeView.input.node as MNodeView).data;
                    int idx = childs.IndexOf(target.id);
                    edgeView.control.UpdateIndex(idx + 1);
                }
            }
        }

        private void UpdateParentConnectionViews()
        {

        }
    }
}

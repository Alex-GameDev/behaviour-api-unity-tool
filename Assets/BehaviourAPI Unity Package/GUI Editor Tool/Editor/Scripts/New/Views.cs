using BehaviourAPI.UnityTool.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;
using BehaviourAPI.Unity.Framework;
using System;
using UnityEditor.Graphs;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using BehaviourAPI.Unity.Framework.Adaptations;
using ConditionNode = BehaviourAPI.UnityTool.Framework.ConditionNode;
using CustomPerception = BehaviourAPI.UnityTool.Framework.CustomPerception;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Editor;
using static Codice.Client.BaseCommands.Import.Commit;
using System.Configuration;
using UnityEditor.UIElements;
using Node = BehaviourAPI.Core.Node;
using System.Linq;

namespace BehaviourAPI.New.Unity.Editor
{
    public class GraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private static string stylePath => BehaviourAPISettings.instance.EditorStylesPath + "graph.uss";

        /// <summary>
        /// The current selected graph
        /// </summary>
        public GraphData graphData;

        /// <summary>
        /// The adapter used to render the current graph
        /// </summary>
        public GraphAdapter _adapter;

        public EditorWindow editorWindow;

        Action<ContextualMenuPopulateEvent> _currentContextualMenuEvent;

        Dictionary<NodeData, NodeDataView> _assetViewMap = new Dictionary<NodeData, NodeDataView>();

        public bool Runtime => false;

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeData> NodeSelected, NodeAdded, NodeRemoved;

        #endregion

        #region -------------------------- SET UP -------------------------------

        public GraphView(EditorWindow window)
        {
            editorWindow = window;
            AddDecorators();
            AddManipulators();

            graphViewChanged = OnGraphViewChanged;
        }

        void AddDecorators()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath));
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());

            if (!Runtime)
            {
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new RectangleSelector());
            }

            nodeCreationRequest = context =>
            {
                if (graphData == null || _adapter == null) return;

                var nodeCreationWindowProvider = ElementCreatorWindow.Create<NodeCreationWindow>(type => CreateNode(type, context.screenMousePosition));
                nodeCreationWindowProvider.SetAdapterType(_adapter.GetType());
                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, nodeCreationWindowProvider);
            };
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            evt.menu.AppendAction("Clear graph", _ => ClearGraph());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (_adapter == null)
            {
                Debug.LogWarning("Cant create connection without adapter");
                return new List<Port>();
            }
            return _adapter.ValidatePorts(ports, startPort, graphData, graphData.graph.CanCreateLoops);
        }

        #endregion

        #region ---------------------- ADD VISUAL ELEMENTS ---------------------------

        public void AddNode(NodeDataView nodeView)
        {
            nodeView.Selected = () => NodeSelected?.Invoke(nodeView.data);
            nodeView.Unselected = () => NodeSelected?.Invoke(null);

            if (Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
            }
            AddElement(nodeView);
            _assetViewMap[nodeView.data] = nodeView;
        }

        public void AddConnectionView(Edge edge)
        {
            if (Runtime)
            {
                edge.capabilities -= Capabilities.Selectable;
                edge.capabilities -= Capabilities.Deletable;
            }
            AddElement(edge);
        }

        #endregion

        #region --------------------------- CHANGE EVENTS -----------------------------

        public void SetGraphData(GraphData data)
        {
            ClearGraph();
            graphData = data;
            _adapter = GraphAdapter.FindAdapter(data.graph);
            _adapter.DrawGraph(data, this);
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.movedElements?.ForEach(OnElementMoved);
            graphViewChange.elementsToRemove?.ForEach(OnElementRemoved);
            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);

            return /* _adapter?.OnViewChanged(this, graphViewChange) ??*/ graphViewChange;
        }

        void OnElementMoved(GraphElement element)
        {
            if (element is NodeDataView nodeView)
            {
                nodeView.data.position = nodeView.GetPosition().position;
                EditorWindow.Instance.OnModifyAsset();
            }
        }

        void OnElementRemoved(GraphElement element)
        {
            if (element is NodeDataView nodeView)
            {
                graphData.nodes.Remove(nodeView.data);
                EditorWindow.Instance.OnModifyAsset();
            }

            if (element is Edge edge)
            {
                var source = (NodeDataView)edge.output.node;
                var target = (NodeDataView)edge.input.node;
                source.OnDisconnected((EdgeView)edge, target, edge.output);
                target.OnDisconnected((EdgeView)edge, source, edge.input);
                EditorWindow.Instance.OnModifyAsset();
            }
        }

        void OnEdgeCreated(Edge edge)
        {
            var source = (NodeDataView)edge.output.node;
            var target = (NodeDataView)edge.input.node;
            source.OnConnected((EdgeView)edge, target, edge.output);
            target.OnConnected((EdgeView)edge, source, edge.input);
            EditorWindow.Instance.OnModifyAsset();
        }

        #endregion

        #region ------------------------------ UTILS ---------------------------------

        public NodeDataView GetViewOf(NodeData nodeAsset) => _assetViewMap[nodeAsset];

        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }

        #endregion           

        #region --------------------------- MODIFY DATA -----------------------------

        void CreateNode(Type type, Vector2 position)
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeData data = new NodeData(type, pos);

            if (data != null)
            {
                _adapter.DrawNode(data, this);
                graphData.nodes.Add(data);
                EditorWindow.Instance.OnModifyAsset();
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }
        }

        void ClearGraph()
        {
            if (graphData != null)
            {
                graphData.nodes.Clear();
                EditorWindow.Instance.OnModifyAsset();
            }
        }

        #endregion
    }

    public class NodeDataView : UnityEditor.Experimental.GraphView.Node
    {
        #region --------------------------- Fields ---------------------------

        public NodeData data;

        public Action Selected;
        public Action Unselected;

        #endregion

        #region ----------------------- Visual elements -----------------------
        public VisualElement RootElement { get; private set; }
        public VisualElement IconElement { get; private set; }
        public VisualElement BorderElement { get; private set; }

        public VisualElement TypeColorTop { get; private set; }
        public VisualElement TypeColorBottom { get; private set; }

        public List<PortView> InputPorts { get; } = new List<PortView>();
        public List<PortView> OutputPorts { get; } = new List<PortView>();

        #endregion

        #region --------------------------- Set up ---------------------------

        public NodeDataView(NodeData data, string path) : base(path)
        {
            this.data = data;

            RootElement = this.Q("node-root");
            IconElement = this.Q("node-icon");
            BorderElement = this.Q("node-border");

            TypeColorTop = this.Q("node-type-color-top");
            TypeColorBottom = this.Q("node-type-color-bottom");

            base.SetPosition(new Rect(this.data.position, Vector2.zero));

            SetUpPorts();

            SetUpDataBinding();
        }


        public void SetUpPorts()
        {
            if (data.node == null || data.node.MaxInputConnections != 0)
            {
                _ = InstantiatePort(Direction.Input, PortOrientation.Bottom);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (data.node == null || data.node.MaxOutputConnections != 0)
            {
                _ = InstantiatePort(Direction.Output, PortOrientation.Top);
            }
            else
            {
                outputContainer.style.display = DisplayStyle.None;
            }
        }

        public void ChangeTypeColor(Color color)
        {
            TypeColorTop.ChangeBackgroundColor(color);
            TypeColorBottom.ChangeBackgroundColor(color);
        }

        private void SetUpDataBinding()
        {
            //var obj = new SerializedObject(Node);
            //var titleInputField = this.Q<TextField>(name: "title-input-field");

            //if (BehaviourEditorWindow.Instance.IsRuntime)
            //{
            //    titleInputField.isReadOnly = true;
            //}

            //titleInputField.bindingPath = "Name";
            //titleInputField.Bind(obj);
        }

        protected PortView InstantiatePort(Direction direction, PortOrientation orientation)
        {
            var isInput = direction == Direction.Input;

            Port.Capacity portCapacity;
            Type portType;

            Node node = data.node;
            if (node != null)
            {
                if (isInput) portCapacity = node.MaxInputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                else portCapacity = node.MaxOutputConnections == -1 ? Port.Capacity.Multi : Port.Capacity.Single;
                portType = isInput ? node.GetType() : node.ChildType;
            }
            else
            {
                portCapacity = Port.Capacity.Multi;
                portType = GetType(); // Any invalid type
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

        public Port GetBestPort(NodeDataView other, Direction direction)
        {
            if (direction == Direction.Input) return InputPorts.FirstOrDefault();
            else return OutputPorts.FirstOrDefault();
        }

        #endregion

        #region --------------------------- Events ---------------------------

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

        public virtual void OnConnected(EdgeView edgeView, NodeDataView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    data.parentIds.Add(other.data.id);
                }
                else
                {
                    data.childIds.Add(other.data.id);
                }
            }

            //if (port.direction == Direction.Output)
            //{
            //    outputEdges.Add(edgeView);
            //    UpdateEdgeViews();
            //}
        }

        public virtual void OnDisconnected(EdgeView edgeView, NodeDataView other, Port port, bool ignoreConnection = false)
        {
            if (!ignoreConnection)
            {
                if (port.direction == Direction.Input)
                {
                    data.parentIds.Remove(other.data.id);
                }
                else
                {
                    data.childIds.Remove(other.data.id);
                }
            }

            //if (port.direction == Direction.Output)
            //{
            //    outputEdges.Remove(edgeView);
            //    UpdateEdgeViews();
            //}
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Visual element that represents a behaviour graph
    /// </summary>
    public class BehaviourGraphView : GraphView
    {
        #region ---------------------------------- Fields ----------------------------------

        NodeCreationSearchWindow _nodeSearchWindow;
        BehaviourGraphEditorWindow editorWindow;

        Action<ContextualMenuPopulateEvent> _currentContextualMenuEvent;

        Dictionary<NodeAsset, NodeView> _assetViewMap = new Dictionary<NodeAsset, NodeView>();

        #endregion

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeAsset> NodeSelected, NodeAdded, NodeRemoved;

        #endregion

        #region -------------------------------- Properties --------------------------------

        public ActionSearchWindow ActionSearchWindow { get; private set; }
        public PerceptionSearchWindow PerceptionSearchWindow { get; private set; }
        public SubgraphSearchWindow SubgraphSearchWindow { get; private set; }

        public NodeSearchWindow NodeSearchWindow { get; private set; }
        
        public GraphAsset GraphAsset { get; private set ; }

        public GraphAdapter _adapter;

        public bool Runtime => BehaviourGraphEditorWindow.IsRuntime;

        #endregion

        public BehaviourGraphView(BehaviourGraphEditorWindow parentWindow)
        {
            editorWindow = parentWindow;
            AddDecorators();
            AddManipulators();
            AddSearchWindows();

            AddStyles();
            graphViewChanged = OnGraphViewChanged;
        }


        #region ---------------------- ADD GRAPH ELEMENTS ---------------------------
        public void AddNodeView(NodeView nodeView)
        {
            nodeView.Selected = (asset) => NodeSelected?.Invoke(asset);

            if (Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                nodeView.capabilities -= Capabilities.Movable;
            }
            AddElement(nodeView);
            _assetViewMap.Add(nodeView.Node, nodeView);
        }

        public void AddConnectionView(Edge edge)
        {
            if (Runtime)
            {
                edge.capabilities -= Capabilities.Selectable;
                edge.capabilities -= Capabilities.Deletable;
                edge.capabilities -= Capabilities.Movable;
            }
            AddElement(edge);
        }

        public NodeView GetViewOf(NodeAsset nodeAsset) => _assetViewMap[nodeAsset];

        #endregion ------------------------------------------------------------------

        #region -------------------------- SET UP -------------------------------

        void AddDecorators()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }

        void AddStyles()
        {
            StyleSheet styleSheet = VisualSettings.GetOrCreateSettings().GraphStylesheet;
            styleSheets.Add(styleSheet);
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
            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                _currentContextualMenuEvent?.Invoke(menuEvt);
            }));
        }
        #endregion ------------------------------------------------------------------

        #region --------------------------- CHANGE EVENTS ---------------------------

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.movedElements?.ForEach(OnElementMoved);
            graphViewChange.elementsToRemove?.ForEach(OnElementRemoved);
            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);

            return _adapter?.OnViewChanged(this, graphViewChange) ?? graphViewChange;
        }

        private void OnEdgeCreated(Edge edge)
        {
            var source = (NodeView)edge.output.node;
            var target = (NodeView)edge.input.node;
            source.OnConnected(Direction.Output, target);
            target.OnConnected(Direction.Input, source);
        }

        void OnElementMoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                nodeView.OnMoved(element.GetPosition().position);
            }
        }

        void OnElementRemoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                GraphAsset.RemoveNode(nodeView.Node);
                _assetViewMap.Remove(nodeView.Node);
                NodeRemoved?.Invoke(nodeView.Node);
            }
            if (element is Edge edge)
            {
                var source = (NodeView)edge.output.node;
                var target = (NodeView)edge.input.node;
                source.OnDisconnected(Direction.Output, target);
                target.OnDisconnected(Direction.Input, source);
            }
        }

        #endregion ------------------------------------------------------------------

        #region --------------------------- CHANGE GRAPH ----------------------------

        public void SetGraph(GraphAsset graph)
        {
            ClearGraph();
            GraphAsset = graph;

            _adapter = GraphAdapter.FindAdapter(graph.Graph);
            _adapter.DrawGraph(graph, this);
            _currentContextualMenuEvent = (menuEvt) => _adapter.BuildGraphContextualMenu(menuEvt, this);

            _nodeSearchWindow.SetEntryHierarchy(_adapter.GetNodeHierarchyEntries());

            _nodeSearchWindow.SetRootType(graph.Graph.NodeType);
        }

        #endregion ------------------------------------------------------------------

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (_adapter == null) return new List<Port>();
            return _adapter.GetValidPorts(ports, startPort, GraphAsset.Graph.CanCreateLoops);
        }

        #region -------------------------- SEARCH WINDOWS ---------------------------

        void AddSearchWindows()
        {

            _nodeSearchWindow = NodeCreationSearchWindow.Create(CreateNode);

            ActionSearchWindow = ActionSearchWindow.Create();
            PerceptionSearchWindow = PerceptionSearchWindow.Create();
            SubgraphSearchWindow = SubgraphSearchWindow.Create(BehaviourGraphEditorWindow.SystemAsset);
            NodeSearchWindow = NodeSearchWindow.Create(BehaviourGraphEditorWindow.SystemAsset);

            nodeCreationRequest = context =>
            {
                if (GraphAsset == null) return;

                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, _nodeSearchWindow);
            };
        }

        #endregion ------------------------------------------------------------------

       
        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }

        void CreateNode(Type type, Vector2 position) 
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeAsset asset = GraphAsset.CreateNode(type, pos);

            if(asset != null)
            {
                _adapter.DrawNode(asset, this);
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }

            NodeAdded?.Invoke(asset);
        }        

        void ClearGraph()
        {
            _assetViewMap.Clear();
            graphElements.ForEach(RemoveElement);
        }

        public void ClearView() => ClearGraph();
    }
}
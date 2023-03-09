using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEditor.PlayerSettings;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Visual element that represents a behaviour graph
    /// </summary>
    public class BehaviourGraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private static string stylePath => BehaviourAPISettings.instance.EditorStylesPath + "graph.uss";

        #region ---------------------------------- Fields ----------------------------------

        NodeCreationWindow _nodeCreationWindow;
        BehaviourEditorWindow editorWindow;

        Action<ContextualMenuPopulateEvent> _currentContextualMenuEvent;

        Dictionary<NodeAsset, NodeView> _assetViewMap = new Dictionary<NodeAsset, NodeView>();

        #endregion

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeAsset> NodeSelected, NodeAdded, NodeRemoved;

        #endregion

        #region -------------------------------- Properties --------------------------------

        public ActionCreationWindow ActionCreationWindow { get; private set; }
        public PerceptionCreationWindow PerceptionCreationWindow { get; private set; }

        public SubgraphSearchWindow SubgraphSearchWindow { get; private set; }
        public NodeSearchWindow NodeSearchWindow { get; private set; }
        public PerceptionSearchWindow PerceptionSearchWindow { get; private set; }

        public GraphAsset GraphAsset { get; private set ; }

        public GraphAdapter _adapter;

        public bool Runtime => BehaviourEditorWindow.Instance.IsRuntime;



        #endregion

        public BehaviourGraphView(BehaviourEditorWindow parentWindow)
        {
            editorWindow = parentWindow;
            AddDecorators();
            AddManipulators();
            AddSearchWindows();
            graphViewChanged = OnGraphViewChanged;

            SubgraphSearchWindow = ScriptableObject.CreateInstance<SubgraphSearchWindow>();
            SubgraphSearchWindow.SetEditorWindow(editorWindow);

            NodeSearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            NodeSearchWindow.SetEditorWindow(editorWindow);

            PerceptionSearchWindow = ScriptableObject.CreateInstance<PerceptionSearchWindow>();
            PerceptionSearchWindow.SetEditorWindow(editorWindow);
        }


        #region ---------------------- ADD GRAPH ELEMENTS ---------------------------
        public void AddNodeView(NodeView nodeView)
        {
            nodeView.Selected = (asset) => NodeSelected?.Invoke(asset);

            if (Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
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

            if(Runtime)
            {
                graphViewChange.elementsToRemove?.Clear();
                graphViewChange.edgesToCreate?.Clear();
                return graphViewChange;
            }

            graphViewChange.elementsToRemove?.ForEach(OnElementRemoved);
            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);

            return _adapter?.OnViewChanged(this, graphViewChange) ?? graphViewChange;
        }

        private void OnEdgeCreated(Edge edge)
        {
            var source = (NodeView)edge.output.node;
            var target = (NodeView)edge.input.node;
            source.OnConnected((EdgeView)edge, target, edge.output);
            target.OnConnected((EdgeView)edge, source, edge.input);
            BehaviourEditorWindow.Instance.OnModifyAsset();
        }

        void OnElementMoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                nodeView.OnMoved(element.GetPosition().position);
                BehaviourEditorWindow.Instance.OnModifyAsset();
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
                source.OnDisconnected((EdgeView)edge, target, edge.output);
                target.OnDisconnected((EdgeView)edge, source, edge.input);
                BehaviourEditorWindow.Instance.OnModifyAsset();
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

            _nodeCreationWindow.SetAdapterType(_adapter.GetType());
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
            _nodeCreationWindow = ScriptableObject.CreateInstance<NodeCreationWindow>();

            ActionCreationWindow = ScriptableObject.CreateInstance<ActionCreationWindow>();
            PerceptionCreationWindow = ScriptableObject.CreateInstance<PerceptionCreationWindow>();

            nodeCreationRequest = context =>
            {
                if (GraphAsset == null) return;
                if (Runtime)
                {
                    return;
                }

                var searchContext = new SearchWindowContext(context.screenMousePosition);
                _nodeCreationWindow.Open((type) => CreateNode(type, context.screenMousePosition), searchContext);
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
        
        public void DuplicateNode(NodeAsset nodeAsset)
        {
            NodeAsset assetCopy = GraphAsset.DuplicateNode(nodeAsset);
            assetCopy.Position += new Vector2(20, 20);

            if (assetCopy != null)
            {
                _adapter.DrawNode(assetCopy, this);
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }
            NodeAdded?.Invoke(assetCopy);
        }

        void ClearGraph()
        {
            _assetViewMap.Clear();
            graphElements.ForEach(RemoveElement);
        }

        public void ClearView() => ClearGraph();

        public void RefreshView()
        {
            ClearGraph();
            _adapter.DrawGraph(GraphAsset, this);
        }
    }
}
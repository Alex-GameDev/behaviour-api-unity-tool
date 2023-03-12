using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BehaviourAPI.Core;
using BehaviourAPI.New.Unity.Editor;
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

        /// <summary>
        /// The current selected graph
        /// </summary>
        public GraphData graphData { get; private set; }

        /// <summary>
        /// The adapter used to render the current graph
        /// </summary>
        public GraphAdapter _adapter;

        #region ---------------------------------- Fields ----------------------------------

        BehaviourEditorWindow editorWindow;

        Dictionary<NodeData, NodeView> _assetViewMap = new Dictionary<NodeData, NodeView>();

        #endregion

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeData> NodeSelected;

        #endregion

        #region -------------------------------- Properties --------------------------------
        public bool Runtime => BehaviourEditorWindow.Instance.IsRuntime;

        public List<NodeView> nodeViews => _assetViewMap.Values.ToList();

        #endregion

        #region --------------------------------- SET UP ----------------------------------
        public BehaviourGraphView(BehaviourEditorWindow parentWindow)
        {
            editorWindow = parentWindow;
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

                var nodeCreationWindowProvider = ElementCreatorWindowProvider.Create<NodeCreationWindow>(type => CreateNode(type, context.screenMousePosition));
                nodeCreationWindowProvider.SetAdapterType(_adapter.GetType());
                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, nodeCreationWindowProvider);
            };

        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            _adapter?.BuildGraphContextualMenu(evt, this);
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
        public void AddNodeView(NodeView nodeView)
        {
            nodeView.Selected = () => NodeSelected?.Invoke(nodeView.Node);
            nodeView.Unselected = () => NodeSelected?.Invoke(null);

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

        #endregion ------------------------------------------------------------------


        #region --------------------------- CHANGE EVENTS ---------------------------

        public void SetGraphData(GraphData data)
        {
            ClearView();
            graphData = data;

            if(graphData != null)
            {
                _adapter = GraphAdapter.FindAdapter(data.graph);
                _adapter.DrawGraph(data, this);
            }
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.movedElements?.ForEach(OnElementMoved);

            if(Runtime)
            {
                graphViewChange.elementsToRemove?.Clear();
                graphViewChange.edgesToCreate?.Clear();
                return graphViewChange;
            }

            bool anyNodeRemoved = false;
            if (graphViewChange.elementsToRemove != null)
            {                
                foreach (var elementToRemove in graphViewChange.elementsToRemove)
                {
                    anyNodeRemoved |= OnElementRemoved(elementToRemove);
                }               
            }

            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);
            BehaviourEditorWindow.Instance.RegisterChanges();

            if (anyNodeRemoved) RefreshProperties();
            return _adapter?.OnViewChanged(this, graphViewChange) ?? graphViewChange;
        }

        void OnElementMoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                nodeView.Node.position = element.GetPosition().position;
            }
        }

        private void OnEdgeCreated(Edge edge)
        {
            var source = (NodeView)edge.output.node;
            var target = (NodeView)edge.input.node;
            source.OnConnected((EdgeView)edge, target, edge.output);
            target.OnConnected((EdgeView)edge, source, edge.input);
        }

        bool OnElementRemoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                graphData.nodes.Remove(nodeView.Node);
                _assetViewMap.Remove(nodeView.Node);
                return true;                
            }
            if (element is Edge edge)
            {
                var source = (NodeView)edge.output.node;
                var target = (NodeView)edge.input.node;
                source.OnDisconnected((EdgeView)edge, target, edge.output);
                target.OnDisconnected((EdgeView)edge, source, edge.input);
            }
            return false;
        }

        public void ClearView()
        {
            _assetViewMap.Clear();
            graphElements.ForEach(RemoveElement);
        }

        public void RefreshView()
        {
            ClearView();
            _adapter.DrawGraph(graphData, this);
        }

        /// <summary>
        /// Call when a node is removed or the order changed to recompute the property paths
        /// </summary>
        public void RefreshProperties()
        {
            Debug.Log("Refresh properties");
            foreach (var view in _assetViewMap.Values)
            {
                view.RefreshProperty();
            }
        }

        #endregion ------------------------------------------------------------------

        #region ------------------------------ UTILS ---------------------------------

        public NodeView GetViewOf(NodeData nodeAsset) => _assetViewMap[nodeAsset];

        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }

        #endregion

        #region --------------------------- MODIFY DATA ----------------------------

        void CreateNode(Type type, Vector2 position)
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeData data = new NodeData(type, pos);

            if (data != null)
            {
                graphData.nodes.Add(data);
                _adapter.DrawNode(data, this);
                BehaviourEditorWindow.Instance.RegisterChanges();
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }
        }

        public void DuplicateNode(NodeData nodeAsset)
        {
            NodeData copy = nodeAsset.Duplicate();
            copy.position += new Vector2(20, 20);

            if (copy != null)
            {
                graphData.nodes.Add(copy);
                BehaviourEditorWindow.Instance.RegisterChanges();
                _adapter.DrawNode(copy, this);
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }
        }

        #endregion ------------------------------------------------------------------       
    }
}
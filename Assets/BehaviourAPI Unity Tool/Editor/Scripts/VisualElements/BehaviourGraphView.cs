using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
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
        NodeView rootNodeView;

        #endregion

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeAsset> NodeSelected, NodeAdded, NodeRemoved;
        public Action<IEnumerable<NodeAsset>> NodesAdded, NodesRemoved; //Bulk actions

        #endregion

        #region -------------------------------- Properties --------------------------------

        public ActionSearchWindow ActionSearchWindow { get; private set; }
        public PerceptionSearchWindow PerceptionSearchWindow { get; private set; }
        public SubgraphSearchWindow SubgraphSearchWindow { get; private set; }

        public NodeSearchWindow NodeSearchWindow { get; private set; }
        
        public GraphAsset GraphAsset { get; private set ; }
        public GraphRenderer Renderer { get; private set; }

        #endregion

        public bool Runtime => BehaviourGraphEditorWindow.IsRuntime;


        public BehaviourGraphView(BehaviourGraphEditorWindow parentWindow)
        {
            editorWindow = parentWindow;
            AddGridBackground();
            AddManipulators();
            AddSearchWindows();
        
            AddStyles();
            graphViewChanged = OnGraphViewChanged;
        }

        public void SetGraph(GraphAsset graph)
        {
            ClearGraph();
            GraphAsset = graph;

            Renderer = GraphRenderer.FindRenderer(graph.Graph);
            Renderer.graphView = this;
            DrawGraph();

            _nodeSearchWindow.SetRootType(graph.Graph.NodeType);
        }

        public void SetRootNode(NodeView nodeView)
        {
            GraphAsset.Nodes.MoveAtFirst(nodeView.Node);
            rootNodeView?.QuitAsStartNode();
            rootNodeView = nodeView;
            rootNodeView?.SetAsStartNode();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            if (Renderer == null) return new List<Port>();
            return Renderer.GetValidPorts(ports, startPort);
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.movedElements?.ForEach(OnElementMoved);
            graphViewChange.elementsToRemove?.ForEach(OnElementRemoved);
            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);

            return Renderer?.OnGraphViewChanged(graphViewChange) ?? graphViewChange;
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
            if(element is NodeView nodeView)
            {
                nodeView.OnMoved(element.GetPosition().position);                
            }
        }

        void OnElementRemoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                GraphAsset.RemoveNode(nodeView.Node);
                NodeRemoved?.Invoke(nodeView.Node);
            }
            if(element is Edge edge)
            {
                var source = (NodeView)edge.output.node;
                var target = (NodeView)edge.input.node;
                source.OnDisconnected(Direction.Output,target);
                target.OnDisconnected(Direction.Input, source);
            }
        }

        #region ---------------- Search windows ----------------

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

        #endregion

        void AddStyles()
        {
            StyleSheet styleSheet = VisualSettings.GetOrCreateSettings().GraphStylesheet;
            styleSheets.Add(styleSheet);
        }

        void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());

            if (!BehaviourGraphEditorWindow.IsRuntime)
            {                
                this.AddManipulator(new SelectionDragger());
                this.AddManipulator(new RectangleSelector());
            }
        }

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
                DrawNodeView(asset);
            }
            else
            {
                Debug.LogWarning("Error creating the node");
            }

            NodeAdded?.Invoke(asset);
        }

        NodeView DrawNodeView(NodeAsset asset)
        {
            //NodeView nodeView = new NodeView(asset, this);
            NodeView nodeView = Renderer.DrawNode(asset);

            if(Runtime)
            {
                nodeView.capabilities -= Capabilities.Deletable;
                nodeView.capabilities -= Capabilities.Movable;
            }

            nodeView.Selected = (asset) => NodeSelected?.Invoke(asset);
            AddElement(nodeView);
            return nodeView;
        }

        void DrawGraph()
        {
            if (GraphAsset == null) return;

            var nodeViews = GraphAsset.Nodes.Select(DrawNodeView).ToList();

            if (nodeViews.Count > 0)
            {
                rootNodeView = nodeViews[0];
                nodeViews[0].SetAsStartNode();
            }

            nodeViews.ForEach(nodeView =>
            {
                for(int i = 0; i < nodeView.Node.Childs.Count; i++)
                {
                    Edge edge = new CustomEdge();
                    var child = nodeView.Node.Childs[i];
                    var childIdx = GraphAsset.Nodes.IndexOf(child);
                    var other = nodeViews[childIdx];
                    AddElement(edge);
                    Port source = (Port)nodeView.outputContainer[0];
                    Port target = (Port)other.inputContainer[0];
                    edge.input = target;
                    edge.output = source;
                    source.Connect(edge);
                    target.Connect(edge);
                    edge.MarkDirtyRepaint();

                    if (Runtime)
                    {
                        edge.capabilities -= Capabilities.Selectable;
                        edge.capabilities -= Capabilities.Deletable;
                        edge.capabilities -= Capabilities.Movable;
                    }
                }
            });
        }

        void ClearGraph()
        {
            graphElements.ForEach(RemoveElement);
        }

        public void ClearView() => ClearGraph();
    }
}
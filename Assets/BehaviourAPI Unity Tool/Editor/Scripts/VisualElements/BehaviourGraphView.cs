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

        BehaviourSystemAsset _systemAsset;
        GraphAsset _graphAsset;

        ActionSearchWindow _actionSearchWindow;
        PerceptionSearchWindow  _perceptionSearchWindow;
        SubgraphSearchWindow _subgraphSearchWindow;

        NodeCreationSearchWindow _nodeSearchWindow;
        BehaviourGraphEditorWindow editorWindow;

        NodeView rootNodeView;

        #endregion

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeAsset> NodeSelected, NodeAdded, NodeRemoved;
        public Action<IEnumerable<NodeAsset>> NodesAdded, NodesRemoved; //Bulk actions

        #endregion

        #region -------------------------------- Properties --------------------------------

        public ActionSearchWindow ActionSearchWindow => _actionSearchWindow;
        public PerceptionSearchWindow PerceptionSearchWindow => _perceptionSearchWindow;
        public SubgraphSearchWindow SubgraphSearchWindow => _subgraphSearchWindow;

        public GraphAsset GraphAsset { get => _graphAsset; set => _graphAsset = value; }

        #endregion


        public BehaviourGraphView(BehaviourGraphEditorWindow parentWindow, BehaviourSystemAsset systemAsset)
        {
            editorWindow = parentWindow;
            _systemAsset = systemAsset;
            AddGridBackground();
            AddManipulators();

            _nodeSearchWindow = AddCreateNodeWindow();
            _actionSearchWindow = AddActionSearchWindow();
            _perceptionSearchWindow = AddPerceptionSearchWindow();

            _subgraphSearchWindow = AddSubgraphSearchWindow();
            
            AddStyles();
            graphViewChanged = OnGraphViewChanged;
        }

        public void SetGraph(GraphAsset graph)
        {
            _graphAsset = graph;
            _nodeSearchWindow.SetRootType(graph.Graph.NodeType);
            DrawGraph();
        }

        public void SetRootNode(NodeView nodeView)
        {
            _graphAsset.Nodes.MoveAtFirst(nodeView.Node);
            rootNodeView?.QuitAsStartNode();
            rootNodeView = nodeView;
            rootNodeView.SetAsStartNode();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            var startPortNodeView = (NodeView)startPort.node;

            ports.ForEach(port =>
            {
                if (startPort.direction == port.direction) return;

                if(startPort.node == port.node) return;

                var portNodeView = (NodeView)port.node;

                if(portNodeView != null)
                {
                    if (startPort.direction == Direction.Input)
                    {
                        if (!port.portType.IsAssignableFrom(startPort.portType)) return;
                        if (startPortNodeView.Node.Parents.Contains(portNodeView.Node)) return;
                    }
                    else
                    {
                        if (!startPort.portType.IsAssignableFrom(port.portType)) return;
                        if (portNodeView.Node.Parents.Contains(startPortNodeView.Node)) return;
                    }
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            graphViewChange.movedElements?.ForEach(OnElementMoved);
            graphViewChange.elementsToRemove?.ForEach(OnElementRemoved);
            graphViewChange.edgesToCreate?.ForEach(OnEdgeCreated);
            return graphViewChange;
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
                _graphAsset.RemoveNode(nodeView.Node);
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

        NodeCreationSearchWindow AddCreateNodeWindow()
        {
            var nodeWindow = ScriptableObject.CreateInstance<NodeCreationSearchWindow>();
            nodeWindow.SetOnSelectEntryCallback(CreateNode);

            nodeCreationRequest = context =>
            {
                if (_graphAsset == null) return;

                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, nodeWindow);
            };
            return nodeWindow;
        }

        ActionSearchWindow AddActionSearchWindow()
        {
            var searchWindow = ScriptableObject.CreateInstance<ActionSearchWindow>();
            return searchWindow;
        }

        PerceptionSearchWindow AddPerceptionSearchWindow()
        {
            var searchWindow = ScriptableObject.CreateInstance<PerceptionSearchWindow>();
            return searchWindow;
        }


        SubgraphSearchWindow AddSubgraphSearchWindow()
        {
            var searchWindow = SubgraphSearchWindow.Create(_systemAsset);
            return searchWindow;
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
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Action search window", (dd) =>
                {
                    _actionSearchWindow.Open((t) => Debug.Log(t.Name));
                });
            }));
        }

        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }

        void CreateNode(Type type, Vector2 position) 
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeAsset asset = _graphAsset.CreateNode(type, pos);

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
            NodeView nodeView = new NodeView(asset, this);
            nodeView.Selected = (asset) => NodeSelected?.Invoke(asset);
            AddElement(nodeView);
            return nodeView;
        }

        void DrawGraph()
        {
            ClearGraph();
            if (_graphAsset == null) return;

            var nodeViews = _graphAsset.Nodes.Select(DrawNodeView).ToList();

            if (nodeViews.Count > 0) nodeViews[0].SetAsStartNode();

            nodeViews.ForEach(nodeView =>
            {
                for(int i = 0; i < nodeView.Node.Childs.Count; i++)
                {
                    Edge edge = new Edge();
                    var child = nodeView.Node.Childs[i];
                    var childIdx = _graphAsset.Nodes.IndexOf(child);
                    var other = nodeViews[childIdx];
                    AddElement(edge);
                    Port source = (Port)nodeView.outputContainer[0];
                    Port target = (Port)other.inputContainer[0];
                    edge.input = target;
                    edge.output = source;
                    source.Connect(edge);
                    target.Connect(edge);
                    edge.MarkDirtyRepaint();
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
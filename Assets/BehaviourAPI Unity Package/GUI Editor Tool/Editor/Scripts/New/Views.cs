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

namespace BehaviourAPI.New.Unity.Editor
{
    public class GraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private static string stylePath => BehaviourAPISettings.instance.EditorStylesPath + "graph.uss";

        public GraphData graphData;

        public GraphAdapter _adapter;

        NodeCreationWindow _nodeCreationWindow;

        public EditorWindow editorWindow;

        Action<ContextualMenuPopulateEvent> _currentContextualMenuEvent;

        Dictionary<NodeData, NodeDataView> _assetViewMap = new Dictionary<NodeData, NodeDataView>();

        #region ---------------------------------- Events ----------------------------------

        public Action<NodeData> NodeSelected, NodeAdded, NodeRemoved;

        #endregion

        public GraphView(EditorWindow window)
        {
            editorWindow = window;
            AddDecorators();
            AddManipulators();

            graphViewChanged = OnGraphViewChanged;
        }

        public void SetGraphData(GraphData data)
        {
            ClearGraph();
            graphData = data;
            _adapter = GraphAdapter.FindAdapter(data.graph);
            _adapter.DrawGraph(data, this);
            _nodeCreationWindow.SetAdapterType(_adapter.GetType());
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

            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(new ContextualMenuManipulator(menuEvt =>
            {
                menuEvt.menu.AppendAction("Clear graph", _ => ClearGraph());
                menuEvt.menu.AppendAction("Assign action", _ => {
                    graphData.nodes[0] = new NodeData(typeof(ConditionNode), Vector2.zero);
                    (graphData.nodes[0].node as ConditionNode).perception = new CustomPerception();

                    //graphData.nodes[0] = new NodeData(typeof(LoopUntilNode), Vector2.zero);
                    //(graphData.nodes[0].node as LoopUntilNode).TargetStatus = Core.Status.Success;

                    //graphData.nodes[0] = new NodeData(typeof(LeafNode), Vector2.zero);
                    //(graphData.nodes[0].node as LeafNode).Ac = Core.Status.Success;

                    EditorWindow.Instance.OnModifyAsset();
                });
            }));

            _nodeCreationWindow = ScriptableObject.CreateInstance<NodeCreationWindow>();

            nodeCreationRequest = context =>
            {
                if (graphData == null) return;

                var searchContext = new SearchWindowContext(context.screenMousePosition);
                _nodeCreationWindow.Open((type) => CreateNode(type, context.screenMousePosition), searchContext);
            };
        }

        #region ---------------------- ADD GRAPH ELEMENTS ---------------------------
        public void AddNodeView(NodeDataView nodeView)
        {
            nodeView.Selected = () => NodeSelected?.Invoke(nodeView.node);

            //if (Runtime)
            //{
            //    nodeView.capabilities -= Capabilities.Deletable;
            //}
            AddElement(nodeView);
            _assetViewMap.Add(nodeView.node, nodeView);
        }

        public void AddConnectionView(Edge edge)
        {
            //if (Runtime)
            //{
            //    edge.capabilities -= Capabilities.Selectable;
            //    edge.capabilities -= Capabilities.Deletable;
            //}
            AddElement(edge);
        }

        public NodeDataView GetViewOf(NodeData nodeAsset) => _assetViewMap[nodeAsset];

        #endregion ------------------------------------------------------------------

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
                nodeView.node.position = nodeView.GetPosition().position;
                EditorWindow.Instance.OnModifyAsset();
            }
        }

        void OnElementRemoved(GraphElement element)
        {
            if (element is NodeDataView nodeView)
            {
                graphData.nodes.Remove(nodeView.node);
                EditorWindow.Instance.OnModifyAsset();
            }
        }

        void OnEdgeCreated(Edge edge)
        {
        }

        Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }


        void CreateNode(Type type, Vector2 position)
        {
            Vector2 pos = GetLocalMousePosition(position - editorWindow.position.position);
            NodeData data = new NodeData(type, position);

            if (data != null)
            {
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

        public void AddNode(NodeDataView nodeView)
        {
            nodeView.Selected = () => NodeSelected?.Invoke(nodeView.node);
            nodeView.Unselected = () => NodeSelected?.Invoke(null);

            AddElement(nodeView);
            _assetViewMap[nodeView.node] = nodeView;
        }
    }

    public class NodeDataView : UnityEditor.Experimental.GraphView.Node
    {
        public NodeData node;

        public Action Selected;
        public Action Unselected;

        public VisualElement RootElement { get; private set; }
        public VisualElement IconElement { get; private set; }
        public VisualElement BorderElement { get; private set; }


        public NodeDataView(NodeData data, string path) : base(path)
        {
            node = data;

            RootElement = this.Q("node-root");
            IconElement = this.Q("node-icon");
            BorderElement = this.Q("node-border");

            SetPosition(new Rect(node.position, Vector2.zero));
        }

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
    }
}

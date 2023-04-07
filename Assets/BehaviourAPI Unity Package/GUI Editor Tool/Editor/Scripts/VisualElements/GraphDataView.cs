using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.Progress;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class GraphDataView : GraphView
    {
        #region -------------------------------------- Static fields --------------------------------------

        private static readonly Vector2 k_miniMapOffset = new Vector2(10, 10);
        private static readonly Vector2 k_miniMapSize = new Vector2(200, 200);

        private static readonly float k_MinZoomScale = 1.5f;
        private static readonly float k_MaxZoomScale = 3f;
        private static string stylePath => BehaviourAPISettings.instance.EditorStylesPath + "graph.uss";

        #endregion

        #region ---------------------------------------- Properties ---------------------------------------
        public GraphData graphData { get; private set; }

        public Action DataChanged { get; set; }
        public Action<List<int>> SelectionNodeChanged { get; set; }

        #endregion

        #region ------------------------------------- Private fields --------------------------------------

        GraphAdapter m_Adapter;

        MiniMap m_Minimap;

        Dictionary<string, MNodeView> m_NodeViewMap = new Dictionary<string, MNodeView>();

        IEdgeConnectorListener m_EdgeConnectorListener;

        SerializedProperty m_CurrentGraphNodesProperty;

        private EditorWindow m_EditorWindow;

        #endregion

        /// <summary>
        /// Create the default graphView
        /// </summary>
        public GraphDataView(EditorWindow editorWindow)
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath));

            GridBackground gridBackground = new GridBackground();
            Insert(0, gridBackground);

            m_Minimap = new MiniMap() { anchored = true };
            m_Minimap.SetPosition(new Rect(k_miniMapOffset, k_miniMapSize));
            RegisterCallback((GeometryChangedEvent evt) =>
            {
                m_Minimap.SetPosition(new Rect(evt.newRect.max - (k_miniMapOffset + k_miniMapSize), k_miniMapSize));
            });

            Add(m_Minimap);

            SetupZoom(ContentZoomer.DefaultMinScale * k_MinZoomScale, ContentZoomer.DefaultMaxScale * k_MaxZoomScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            m_EdgeConnectorListener = new CustomEdgeConnector<EdgeView>(OnEdgeCreated, OnEdgeCreatedOutsidePort);
            nodeCreationRequest = HandleNodeCreationCall;
            graphViewChanged = HandleGraphViewChanged;
            m_EditorWindow = editorWindow;
        }

        /// <summary>
        /// Change the selected graph
        /// </summary>
        public void UpdateGraph(GraphData graphData, SerializedProperty serializedProperty = null)
        {
            ClearView();
            this.graphData = graphData;

            if (graphData == null)
            {
                m_CurrentGraphNodesProperty = null;
                return;
            }

            m_Adapter = GraphAdapter.GetAdapter(graphData.graph.GetType());
            m_CurrentGraphNodesProperty = serializedProperty;
            DrawGraph();         
        }

        private void OnEdgeCreatedOutsidePort(EdgeView edge, Vector2 position)
        {
        }

        private void OnEdgeCreated(EdgeView edge)
        {
            var edgesToDelete = new List<Edge>();

            if(edge.input.capacity == Port.Capacity.Single)
            {
                foreach(Edge connection in edge.input.connections)
                {
                    if (connection != edge) edgesToDelete.Add(connection);
                }
            }

            if(edge.output.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in edge.output.connections)
                {
                    if (connection != edge) edgesToDelete.Add(connection);
                }
            }

            if(edgesToDelete.Count > 0) DeleteElements(edgesToDelete);


            edge.input.Connect(edge);
            edge.output.Connect(edge);

            CreateConnection(edge);
        }

        private void HandleNodeCreationCall(NodeCreationContext ctx)
        {
            var nodeCreationWindowProvider = ElementCreatorWindowProvider.Create<NodeCreationWindow>(type => CreateNode(type, ctx.screenMousePosition));         
            nodeCreationWindowProvider.SetHierarchy(m_Adapter.NodeHierarchy);
            SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), nodeCreationWindowProvider);
        }

        private GraphViewChange HandleGraphViewChanged(GraphViewChange change)
        {
            List<MNodeView> nodeViewsRemoved = new List<MNodeView>();
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is MNodeView nodeView)
                    {
                        graphData.nodes.Remove(nodeView.data);
                        m_NodeViewMap.Remove(nodeView.data.id);
                        nodeViewsRemoved.Add(nodeView);
                    }
                    else if (element is EdgeView edge)
                    {
                        var source = (MNodeView)edge.output.node;
                        var target = (MNodeView)edge.input.node;
                        source.OnDisconnected(edge);
                        target.OnDisconnected(edge);
                    }
                }
                SelectionNodeChanged?.Invoke(new List<int>());
            }
            if (change.movedElements != null)
            {
                foreach(var element in change.movedElements)
                {
                    if(element is MNodeView nodeView)
                    {
                        nodeView.OnMoved();
                    }
                }
            }

            foreach (var nodeRemoved in nodeViewsRemoved) nodeRemoved.OnDeleted();


            m_CurrentGraphNodesProperty.serializedObject.Update();

            if (nodeViewsRemoved.Count > 0)
            {
                RefreshProperties();
                RefreshViews();
            }

            DataChanged?.Invoke();

            return change;           
        }

        public void UpdateProperties()
        {
            if(m_CurrentGraphNodesProperty != null)
                m_CurrentGraphNodesProperty.serializedObject.Update();

            DataChanged?.Invoke();
            Debug.Log("Updated");
        }

        private void CreateNode(Type type, Vector2 pos)
        {
            if (m_CurrentGraphNodesProperty == null) return;

            var localPos = contentViewContainer.WorldToLocal(pos - m_EditorWindow.position.position);
            NodeData data = new NodeData(type, localPos);
            if(data != null)
            {
                graphData.nodes.Add(data);
                m_CurrentGraphNodesProperty.serializedObject.Update();
                DrawNode(data);
            }
        }

        private void CreateConnection(EdgeView edgeView)
        {
            var source = edgeView.output.node as MNodeView;
            var target = edgeView.input.node as MNodeView;

            source.OnConnected(edgeView);
            target.OnConnected(edgeView);

            AddElement(edgeView);
            UpdateProperties();
        }



        private void ClearView()
        {
            m_NodeViewMap.Clear();
            graphElements.ForEach(RemoveElement);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> validPorts = new List<Port>();
            MNodeView startNodeView = startPort.node as MNodeView;

            if (startNodeView == null) return validPorts;

            var bannedNodes = new HashSet<NodeData>();

            if (!graphData.graph.CanCreateLoops)
            {
                bannedNodes = startPort.direction == Direction.Input ? graphData.GetChildPathing(startNodeView.data) :
                graphData.GetParentPathing(startNodeView.data);
            }
            else if (!graphData.graph.CanRepeatConnection)
            {
                bannedNodes = startPort.direction == Direction.Input ? graphData.GetDirectParents(startNodeView.data) :
                graphData.GetDirectChilds(startNodeView.data);
            }

            foreach (Port port in ports)
            {
                if (startPort.direction == port.direction) continue;
                if (startPort.node == port.node) continue;

                MNodeView otherNodeView = port.node as MNodeView;

                if (bannedNodes.Contains(otherNodeView.data)) continue;
                if (startPort.direction == Direction.Input && !port.portType.IsAssignableFrom(startPort.portType)) continue;
                if (startPort.direction == Direction.Output && !startPort.portType.IsAssignableFrom(port.portType)) continue;


                validPorts.Add(port);
            }
            return validPorts;
        }

        #region ------------------------------- Draw elements -------------------------------

        private void DrawGraph()
        {
            if (graphData == null) return;

            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                DrawNode(graphData.nodes[i]);
            }

            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                NodeData nodeData = graphData.nodes[i];
                DrawConnections(nodeData);               
            }
        }

        private void DrawNode(NodeData nodeData)
        {
            NodeDrawer drawer = NodeDrawer.Create(nodeData.node);
            var index = graphData.nodes.IndexOf(nodeData);
            MNodeView mNodeView = new MNodeView(nodeData, drawer, this, m_EdgeConnectorListener, m_CurrentGraphNodesProperty.GetArrayElementAtIndex(index));
            m_NodeViewMap.TryAdd(nodeData.id, mNodeView);
            AddElement(mNodeView);
        }

        private void DrawConnections(NodeData nodeData)
        {
            if (nodeData.node != null && nodeData.node.MaxInputConnections == 0) return;

            if (!m_NodeViewMap.TryGetValue(nodeData.id, out MNodeView sourceNodeView)) return;

            for (int i = 0; i < nodeData.childIds.Count; i++)
            {
                if (!m_NodeViewMap.TryGetValue(nodeData.childIds[i], out  MNodeView targetNodeView)) return;

                Port sourcePort = sourceNodeView.GetBestPort(targetNodeView, Direction.Output);
                Port targetPort = targetNodeView.GetBestPort(sourceNodeView, Direction.Input);
                EdgeView edge = sourcePort.ConnectTo<EdgeView>(targetPort);
                AddElement(edge);

                sourceNodeView.OnConnected(edge, false);
                targetNodeView.OnConnected(edge, false);
            }
        }

        #region --------------------------------- Selection ---------------------------------

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            OnSelectionChange();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            OnSelectionChange();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            OnSelectionChange();
        }

        private void OnSelectionChange()
        {
            List<int> selectedNodeIndex = new List<int>();
            for(int i = 0; i < selection.Count; i++)
            {
                if (selection[i] is MNodeView nodeView)
                {
                    int id = graphData.nodes.IndexOf(nodeView.data);
                    selectedNodeIndex.Add(id);
                }
            }
            SelectionNodeChanged?.Invoke(selectedNodeIndex);
        }

        #endregion

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", dma =>
            {
                nodeCreationRequest(new NodeCreationContext() { screenMousePosition = dma.eventInfo.mousePosition + m_EditorWindow.position.position, target = null, index = -1 });
            }, (m_CurrentGraphNodesProperty != null) ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled
            );
            evt.menu.AppendAction("Auto layout", _ => AutoLayoutGraph());
        }

        private void AutoLayoutGraph()
        {
            m_Adapter.AutoLayout(graphData);
            m_CurrentGraphNodesProperty.serializedObject.Update();
            RefreshViews();
        }

        /// <summary>
        /// Change the index of a node.
        /// </summary>
        public void ReorderNode(MNodeView nodeView)
        {
            graphData.nodes.MoveAtFirst(nodeView.data);
            m_CurrentGraphNodesProperty.serializedObject.Update();

            RefreshViews();
        }

        private void RefreshViews()
        {
            for(int i = 0; i < graphData.nodes.Count; i++)
            {
                var data = graphData.nodes[i];
                var view = m_NodeViewMap[data.id];
                view.RefreshView();
            }
        }

        private void RefreshProperties()
        {
            for (int i = 0; i < graphData.nodes.Count; i++)
            {
                var data = graphData.nodes[i];
                var view = m_NodeViewMap[data.id];
                view.UpdateSerializedProperty(m_CurrentGraphNodesProperty.GetArrayElementAtIndex(i));
            }
        }

        public void RefreshSelectedNodesProperties()
        {
            foreach (var elem in selection)
            {
                if(elem is MNodeView nodeView)
                {
                    nodeView.RefreshDisplay();
                }
            }
        }

        #endregion
    }
}

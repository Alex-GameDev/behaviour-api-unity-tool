using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Unity.Runtime;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Visual element that represents a behaviour graph
    /// </summary>
    public class BehaviourGraphView : GraphView
    {
        GraphAsset GraphAsset;
        HierarchySearchWindow searchWindow;
        EditorWindow editorWindow;

        public Action<NodeAsset> NodeSelected { get; set; }

        public BehaviourGraphView(EditorWindow parentWindow)
        {
            editorWindow = parentWindow;
            AddGridBackground();
            AddManipulators();
            AddCreateNodeWindow();
            AddStyles();
            graphViewChanged = OnGraphViewChanged;
            //DrawGraph();
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

                if (startPort.direction == Direction.Input)
                {
                    if (!startPort.portType.IsSubclassOf(port.portType)) return;
                    if (startPortNodeView.Node.Parents.Contains(portNodeView.Node)) return;
                }
                else
                {
                    if (!port.portType.IsSubclassOf(startPort.portType)) return;
                    if (portNodeView.Node.Parents.Contains(startPortNodeView.Node)) return;
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
                nodeView.Node.Position = element.GetPosition().position;
            }
        }

        void OnElementRemoved(GraphElement element)
        {
            if (element is NodeView nodeView)
            {
                GraphAsset.RemoveNode(nodeView.Node);
            }
            if(element is Edge edge)
            {
                var source = (NodeView)edge.output.node;
                var target = (NodeView)edge.input.node;
                source.OnDisconnected(Direction.Output,target);
                target.OnDisconnected(Direction.Input, source);
            }
        }

        void AddCreateNodeWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<HierarchySearchWindow>();
                searchWindow.SetRootType(typeof(BTNode));
                searchWindow.SetOnSelectEntryCallback(CreateNode);
            }

            nodeCreationRequest = context =>
            {
                var searchContext = new SearchWindowContext(context.screenMousePosition);
                SearchWindow.Open(searchContext, searchWindow);
            };
        }

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
        }

        NodeView DrawNodeView(NodeAsset asset)
        {
            NodeView nodeView = new NodeView(asset);
            nodeView.Selected = (asset) => NodeSelected?.Invoke(asset);
            AddElement(nodeView);
            return nodeView;
        }

        void DrawGraph()
        {
            if (GraphAsset == null) return;

            var nodeViews = GraphAsset.Nodes.Select(DrawNodeView).ToList();

            nodeViews.ForEach(nodeView =>
            {
                for(int i = 0; i < nodeView.Node.Childs.Count; i++)
                {
                    Edge edge = new Edge();
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
                }
            });
        }
    }
}
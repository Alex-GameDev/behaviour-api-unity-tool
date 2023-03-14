using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine.UIElements;

using LeafNode = BehaviourAPI.Unity.Framework.Adaptations.LeafNode;
using System.ComponentModel;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(BehaviourTree))]
    public class BehaviourTreeAdapter : GraphAdapter
    {
        #region ------------------- Rendering -------------------

        NodeView _rootView;

        public override List<Type> ExcludedTypes => new List<Type> {
            typeof(BehaviourTrees.ConditionNode),
            typeof(BehaviourTrees.LeafNode),
            typeof(BehaviourTrees.SwitchDecoratorNode)
        };

        public override List<Type> MainTypes => new List<Type> {
            typeof(CompositeNode),
            typeof(DecoratorNode),
            typeof(LeafNode)
        };

        protected override NodeView GetLayout(NodeData asset, BehaviourGraphView graphView) => new TreeNodeView(asset, graphView);

        protected override void DrawGraphDetails(GraphData data, BehaviourGraphView graphView)
        {
            var firstNode = data.nodes.FirstOrDefault();
            if (firstNode != null) ChangeRootNode(graphView.nodeViews.Find(n => n.Node == firstNode));
        }

        protected override void DrawNodeDetails(NodeView nodeView)
        {
            var node = nodeView.Node.node;
            if (node is BehaviourTrees.LeafNode)
            {
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.LeafNodeColor);
            }
            else if (node is CompositeNode)
            {
                nodeView.IconElement.Enable();
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.CompositeColor);
                if (node is SequencerNode) nodeView.IconElement.Add(new Label("-->"));
                else if (node is SelectorNode) nodeView.IconElement.Add(new Label("?"));
            }
            else if (node is DecoratorNode)
            {
                nodeView.IconElement.Enable();
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.DecoratorColor);
                nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().ToUpper()));
            }
        }

        protected override void SetUpGraphEditorContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order childs by position (x)", _ =>
            {
                graph.graphData.OrderAllChildNodes((n) => n.position.x);
                BehaviourEditorWindow.Instance.RegisterChanges();
                graph.nodeViews.ForEach(n => n.UpdateEdgeViews());
            });
        }

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set root node", _ => SetRootNode(node), _ => (node != _rootView).ToMenuStatus());
            menuEvt.menu.AppendAction("Order childs by position (x)", _ =>
            {
                node.GraphView.graphData.OrderChildNodes(node.Node, (n) => n.position.x);
                BehaviourEditorWindow.Instance.RegisterChanges();
                node.UpdateEdgeViews();
            },
                (node.Node.childIds.Count > 1).ToMenuStatus()
            );
        }


        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            var rootNode = graphView.graphData.nodes.Find(n => n.parentIds.Count == 0);

            if (rootNode != null)
            {
                graphView.graphData.nodes.MoveAtFirst(rootNode);
                var view = graphView.nodes.Select(n => n as NodeView).ToList().Find(n => n.Node == rootNode);
                ChangeRootNode(view);
            }
            return change;
        }

        void SetRootNode(NodeView nodeView)
        {
            nodeView.DisconnectPorts(nodeView.inputContainer);
            ChangeRootNode(nodeView, true);
        }

        void ChangeRootNode(NodeView newRootNode, bool changeData = false)
        {
            if (newRootNode == null || newRootNode.Node.parentIds.Count > 0) return;

            var graphView = newRootNode.GraphView;

            if (_rootView != null)
            {
                _rootView.inputContainer.Enable();
                _rootView.RootElement.Disable();
            }

            _rootView = newRootNode;
            if (_rootView != null)
            {
                if (changeData)
                {
                    graphView.graphData.nodes.MoveAtFirst(_rootView.Node);
                    BehaviourEditorWindow.Instance.RegisterChanges();
                    graphView.RefreshProperties();
                }

                _rootView.inputContainer.Disable();
                _rootView.RootElement.Enable();
            }
        }

        #endregion

        #region ------------------------------ Generate code ------------------------------


        #endregion
    }

    public class TreeNodeView : NodeView
    {
        public TreeNodeView(NodeData node, BehaviourGraphView graphView) : base(node, graphView, BehaviourAPISettings.instance.EditorLayoutsPath + "/Nodes/Tree Node.uxml")
        {
        }

        public override string LayoutPath => "/Nodes/Tree Node.uxml";

        public override void SetUpPorts()
        {
            if (Node.node == null || Node.node.MaxInputConnections != 0)
            {
                var port = InstantiatePort(Direction.Input, PortOrientation.Bottom);
            }
            else
            {
                inputContainer.style.display = DisplayStyle.None;
            }

            if (Node.node == null || Node.node.MaxOutputConnections != 0)
            {
                var port = InstantiatePort(Direction.Output, PortOrientation.Top);
            }
            else
            {
                outputContainer.style.display = DisplayStyle.None;
            }
        }

        public override void OnConnected(EdgeView edgeView, NodeView other, Port port, bool ignoreConnection = false)
        {
            base.OnConnected(edgeView, other, port, ignoreConnection);

            if (GraphView.Runtime && port.direction == Direction.Output)
            {
                if (other.Node.node is BTNode btNode)
                {
                    btNode.LastExecutionStatusChanged += (status) => edgeView.control.UpdateStatus(status);
                    edgeView.control.UpdateStatus(btNode.LastExecutionStatus);
                }
            }
        }

        public void ResetStatus()
        {
            outputEdges.ForEach(edge =>
            {
                edge.control.UpdateStatus(Status.None);
                (edge.input.node as TreeNodeView).ResetStatus();
            });
        }
    }
}

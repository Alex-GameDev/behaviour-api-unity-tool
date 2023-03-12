using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine.UIElements;

using LeafNode = BehaviourAPI.Unity.Framework.Adaptations.LeafNode;

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

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {
            var firstNode = graphAsset.Nodes.FirstOrDefault();
            if (firstNode != null) ChangeRootNode(nodeViews.Find(n => n.Node == firstNode));
        }

        protected override NodeView GetLayout(NodeData asset, BehaviourGraphView graphView) => new TreeNodeView(asset, graphView);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set root node", _ => SetRootNode(node), _ => (node != _rootView).ToMenuStatus());
            menuEvt.menu.AppendAction("Order childs by position (x)", _ => node.OrderChilds(n => n.Position.x), (node.Node.Childs.Count > 1).ToMenuStatus());
        }

        protected override void SetUpDetails(NodeView nodeView)
        {
            var node = nodeView.Node.Node;
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
            else if(node is DecoratorNode)
            {
                nodeView.IconElement.Enable();
                nodeView.ChangeTypeColor(BehaviourAPISettings.instance.DecoratorColor);
                nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().ToUpper()));
            }       
        }                

        // Reload the root data when the old one is removed
        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            var rootNode = graphView.GraphAsset.Nodes.Find(n => n.Parents.Count == 0);

            if (rootNode != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(rootNode);
                var view = graphView.nodes.Select(n => n as NodeView).ToList().Find(n => n.Node == rootNode);
                ChangeRootNode(view);
            }
            return change;
        }

        void SetRootNode(NodeView nodeView)
        {
            nodeView.DisconnectPorts(nodeView.inputContainer);
            ChangeRootNode(nodeView);
        }

        void ChangeRootNode(NodeView newRootNode)
        {
            if (newRootNode == null || newRootNode.Node.Parents.Count > 0) return;

            var graphView = newRootNode.GraphView;

            if (_rootView != null)
            {
                _rootView.inputContainer.Enable();
                _rootView.RootElement.Disable();
            }

            _rootView = newRootNode;
            if (_rootView != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(_rootView.Node);
                BehaviourEditorWindow.Instance.OnModifyAsset();

                _rootView.inputContainer.Disable();
                _rootView.RootElement.Enable();
            }
        }

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Order all node's child by position (x)", 
                _ =>
                {
                    graph.nodes.ForEach(n => (n as NodeView).OrderChilds(n => n.Position.x, false));
                    BehaviourEditorWindow.Instance.OnModifyAsset();
                });
        }

        #endregion
    }
}

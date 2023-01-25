using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.BehaviourTrees.Decorators;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine.UIElements;
using LeafNode = BehaviourAPI.Unity.Runtime.LeafNode;

namespace BehaviourAPI.Unity.Editor
{
    public class BehaviourTreeAdapter : GraphAdapter
    {
        #region --------------- Assets generation ---------------

        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region ---------------- Code generation ----------------

        public override void ConvertAssetToCode(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            scriptTemplate.AddLine($"// BehaviourTree - {graphName}:");

            var rootNode = graphAsset.Nodes.FirstOrDefault();

            if (rootNode != null)
            {
                var rootNodeName = AddNode(rootNode, scriptTemplate, graphName);
                scriptTemplate.AddLine($"{graphName}.SetRootNode({rootNodeName});");
            }
        }

        public override string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        {
            if(graphAsset.Graph is BehaviourTree behaviourTree)
            {
                scriptTemplate.AddUsingDirective(typeof(BehaviourTree).Namespace);
                scriptTemplate.AddUsingDirective($"{nameof(LeafNode)} = {typeof(LeafNode).FullName}");
                return scriptTemplate.AddVariableInstantiationLine(behaviourTree.TypeName(), graphAsset.Name, graphAsset);
            }
            else
            {
                return null;
            }
        }

        string AddNode(NodeAsset node, ScriptTemplate template, string graphName)
        {
            var btNode = node.Node as BTNode;
            var nodeName = node.Name ?? btNode.TypeName().ToLower();
            string typeName = btNode.TypeName();

            var method = string.Empty;

            if (btNode is CompositeNode composite)
            {
                var args = new List<string>();
                args.Add(composite.IsRandomized.ToCodeFormat());

                foreach (var child in node.Childs)
                {
                    var childName = AddNode(child, template, graphName);
                    if (childName != null) args.Add(childName);
                }
                method = $"CreateComposite<{typeName}>({args.Join()})";
            }
            else if (btNode is DecoratorNode decorator)
            {
                var childName = AddNode(node.Childs.FirstOrDefault(), template, graphName) ?? "null /* Missing child */";

                var propertyCode = AddDecoratorProperties(decorator, template);
                method = $"CreateDecorator<{typeName}>({childName}){propertyCode}";                
            }
            else if (btNode is LeafNode leaf)
            {
                var actionCode = GenerateActionCode(leaf.Action, template) ?? "null /* Missing action */";
                method = $"CreateLeafNode({actionCode})";                
            }
            return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{method})");
        }

        string AddDecoratorProperties(DecoratorNode decorator, ScriptTemplate scriptTemplate)
        {
            if (decorator is IteratorNode iterator)
            {
                return $".SetIterations({iterator.Iterations})";
            }
            else if (decorator is LoopUntilNode loopUntil)
            {
                return $".SetTargetStatus({loopUntil.TargetStatus}).SetMaxIterations({loopUntil.MaxIterations})";
            }
            else if (decorator is ConditionNode conditionNode)
            {
                return $".SetPerception({GeneratePerceptionCode(conditionNode.Perception, scriptTemplate) ?? "null /* #Perception */"})";
            }
            else
            {
                return "";
            }
        }

        #endregion

        #region ------------------- Rendering -------------------

        NodeView _rootView;

        protected override List<Type> ExcludedTypes => new List<Type> { 
            typeof(ConditionDecoratorNode), 
            typeof(SwitchDecoratorNode) 
        };

        protected override List<Type> MainTypes => new List<Type> { 
            typeof(CompositeNode), 
            typeof(DecoratorNode), 
            typeof(LeafNode) 
        };       

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {
            var firstNode = graphAsset.Nodes.FirstOrDefault();
            if (firstNode != null) ChangeRootNode(nodeViews.Find(n => n.Node == firstNode));
        }

        protected override string GetNodeLayoutPath(NodeAsset _) => AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendSeparator();
            menuEvt.menu.AppendAction("Set root node", _ => SetRootNode(node), _ => (node == _rootView).ToMenuStatus());
            menuEvt.menu.AppendAction("Order childs by position (x)", _ => node.Node.OrderChilds(n => n.Position.x), (node.Node.Childs.Count > 1).ToMenuStatus());
        }

        protected override void SetUpPorts(NodeView nodeView)
        {
            if (nodeView.Node.Node.MaxInputConnections != 0)
            {
                CreatePort(nodeView, nodeView.Node.Node.MaxInputConnections, Direction.Input, nodeView.Node.Node.GetType());
            }
            else
            {
                nodeView.inputContainer.style.display = DisplayStyle.None;
            }                

            if (nodeView.Node.Node.MaxOutputConnections != 0)
            {
                CreatePort(nodeView, nodeView.Node.Node.MaxOutputConnections, Direction.Output, nodeView.Node.Node.ChildType);
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;
        }

        void CreatePort(NodeView nodeView, int maxConnections, Direction direction, Type type)
        {
            var port = nodeView.InstantiatePort(Orientation.Vertical, direction, maxConnections > 1 ? Port.Capacity.Multi : Port.Capacity.Single, type);
            port.portName = "";
            port.style.flexDirection = direction == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
            nodeView.inputContainer.Add(port);
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

                _rootView.inputContainer.Disable();
                _rootView.RootElement.Enable();
            }
        }

        #endregion
    }
}

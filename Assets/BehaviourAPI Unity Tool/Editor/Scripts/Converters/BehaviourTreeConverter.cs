using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Action = BehaviourAPI.Core.Actions.Action;
using LeafNode = BehaviourAPI.BehaviourTrees.LeafNode;
using UnityAction = BehaviourAPI.Unity.Runtime.UnityAction;

namespace BehaviourAPI.Unity.Editor
{
    [CustomConverter(typeof(BehaviourTree))]
    public class BehaviourTreeConverter : GraphConverter
    {
        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(BehaviourTree)) return null;

            return GraphAsset.Create("new graph", typeof(BehaviourTree));
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(BehaviourTree)) return;

            graphName = scriptTemplate.FindVariableName(asset);

            scriptTemplate.AddLine($"// Behaviour tree - {graphName}:");

            var rootNode = asset.Nodes.FirstOrDefault();

            if (rootNode != null)
            {
                var rootNodeName = AddNode(rootNode, scriptTemplate);
                scriptTemplate.AddLine($"{graphName}.SetRootNode({rootNodeName});");
            }
            else
                scriptTemplate.AddLine($"//Behaviour tree ({graphName}) is empty;");
        }

        public override string AddCreateGraphLine(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            scriptTemplate.AddUsingDirective(typeof(BehaviourTree).Namespace);
            scriptTemplate.AddUsingDirective($"{nameof(LeafNode)} = {typeof(LeafNode).FullName}");
            return base.AddCreateGraphLine(asset, scriptTemplate);
        }

        private string AddNode(NodeAsset node, ScriptTemplate scriptTemplate)
        {
            var btNode = node.Node as BTNode;
            var nodeName = node.Name;

            if (string.IsNullOrEmpty(nodeName)) nodeName = btNode.TypeName().ToLower();


            if(btNode is CompositeNode composite)
            {
                var childNames = node.Childs.Select(n => AddNode(n, scriptTemplate)).ToList();
                childNames.RemoveAll(s => string.IsNullOrWhiteSpace(s));
                string typeName = composite.TypeName();

                List<string> arguments = new List<string>();
                arguments.Add(composite.IsRandomized.ToCodeFormat());
                arguments.AddRange(childNames);

                return scriptTemplate.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateComposite<{typeName}>({string.Join(", ", arguments)})");
            }
            else if(btNode is DecoratorNode decorator)
            {            
                var childName = AddNode(node.Childs.FirstOrDefault(), scriptTemplate);
                string typeName = decorator.TypeName();

                if (childName == null) childName = "null /* Error */";

                var propertyCode = AddDecoratorProperties(decorator, scriptTemplate);
                return scriptTemplate.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateDecorator<{typeName}>({childName}){propertyCode}");
            }
            else if(btNode is LeafNode leaf)
            {
                string typeName = leaf.TypeName();
                var actionCode = GetActionCode(leaf.Action, scriptTemplate);
                return scriptTemplate.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateLeafNode({actionCode})");
            }
            else
            {
                Debug.Log("NULL");
                return null;
            }            
        }   
        
        private string AddDecoratorProperties(DecoratorNode decorator, ScriptTemplate scriptTemplate)
        {
            if(decorator is IteratorNode iterator)
            {
                return $".SetIterations({iterator.Iterations})";
            }
            else if(decorator is LoopUntilNode loopUntil)
            {
                return $".SetTargetStatus({loopUntil.TargetStatus}).SetMaxIterations({loopUntil.MaxIterations})";
            }
            else if(decorator is ConditionNode conditionNode)
            {
                return $".SetPerception({GetPerceptionCode(conditionNode.Perception, scriptTemplate) ?? "null /* #Perception */"})";
            }
            else
            {
                return "";
            }

        }
    }
}

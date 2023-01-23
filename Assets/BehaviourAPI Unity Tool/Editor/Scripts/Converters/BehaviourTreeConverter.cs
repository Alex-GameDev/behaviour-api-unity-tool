using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Runtime;
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
        private string graphName;

        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(BehaviourTree)) return null;

            return GraphAsset.Create("new graph", typeof(BehaviourTree));
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(BehaviourTree)) return;

            var rootNode = asset.Nodes.FirstOrDefault();

            if (rootNode != null)
            {
                AddNode(rootNode, scriptTemplate);
            }
            else
                Debug.Log("ERROR");

            scriptTemplate.AddLine("");
        }
         
        private string AddNode(NodeAsset node, ScriptTemplate scriptTemplate)
        {
            var bTNode = node.Node as BTNode;

            if(bTNode is CompositeNode composite)
            {
                var childNames = node.Childs.Select(n => AddNode(n, scriptTemplate)).ToList();
                childNames.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                //scriptTemplate.AddVariableDeclarationLine(node.name);
                //scriptTemplate.BeginMethod($"{graphName}.CreateComposite<{composite.GetType().Name}>");
                //scriptTemplate.AddParameter(composite.IsRandomized.ToString().ToLower());
                //scriptTemplate.AddParameters(childNames);
                //scriptTemplate.CloseMethodOrVariableAsignation();
                return node.Name;
            }
            else if(bTNode is DecoratorNode decorator)
            {            
                var childName = AddNode(node.Childs.FirstOrDefault(), scriptTemplate);

                //scriptTemplate.AddVariableDeclarationLine(node.Name);
                //scriptTemplate.BeginMethod($"{graphName}.CreateDecorator<{decorator.GetType().Name}>");
                //scriptTemplate.AddParameter(childName);
                //scriptTemplate.CloseMethodOrVariableAsignation();
                return node.Name;
            }
            else if(bTNode is LeafNode leaf)
            {
                //scriptTemplate.AddVariableDeclarationLine(node.Name);
                //scriptTemplate.BeginMethod($"{graphName}.CreateLeafNode");
                //AddAction(leaf.Action, scriptTemplate);
                return node.Name;
            }
            else
            {
                Debug.Log("NULL");
                return null;
            }            
        }       
    }
}

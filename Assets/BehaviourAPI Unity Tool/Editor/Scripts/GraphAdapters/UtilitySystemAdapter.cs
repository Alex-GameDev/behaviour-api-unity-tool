using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Runtime.Extensions;
using BehaviourAPI.UtilitySystems;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using UtilityAction = BehaviourAPI.Unity.Framework.Adaptations.UtilityAction;
using CustomFunction = BehaviourAPI.Unity.Framework.Adaptations.CustomFunction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(UtilitySystem))]
    public class UtilitySystemAdapter : GraphAdapter
    {
        //public override void ConvertAssetToCode(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        //{
        //    var graphName = scriptTemplate.FindVariableName(graphAsset);

        //    var graph = graphAsset.Graph;
        //    scriptTemplate.AddLine($"// UtilitySystem - {graphName}:");

        //    var utilityElements = graphAsset.Nodes.FindAll(n => n.Node is UtilitySelectableNode);
        //    var factors = graphAsset.Nodes.FindAll(n => n.Node is Factor);

        //    foreach(var factor in factors)
        //    {
        //        if (scriptTemplate.FindVariableName(factor) == null)
        //        {
        //            AddFactor(factor, scriptTemplate, graphName);
        //        }                    
        //    }

        //    scriptTemplate.AddLine("");

        //    foreach (var utilityElement in utilityElements)
        //    {
        //        if (scriptTemplate.FindVariableName(utilityElement) == null)
        //        {
        //            AddUtilityElement(utilityElement, scriptTemplate, graphName);
        //        }
        //    }

        //    var defaultElement = utilityElements.FirstOrDefault();
        //    if (defaultElement != null)
        //    {
        //        var defaultElementName = scriptTemplate.FindVariableName(defaultElement);
        //        if (!string.IsNullOrEmpty(defaultElementName))
        //        {
        //            scriptTemplate.AddLine($"{graphName}.SetDefaultUtilityElement({defaultElementName});");
        //        }
        //    }
        //}

        //string AddFactor(NodeAsset node, ScriptTemplate template, string graphName)
        //{
        //    Factor factor = node.Node as Factor;
        //    var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : factor.TypeName().ToLower();
        //    string typeName = factor.TypeName();

        //    var method = string.Empty;

        //    if(factor is VariableFactor variableFactor)
        //    {
        //        var functionCode = GenerateSerializedMethodCode(variableFactor.variableFunction, template) ?? "() => 0f /*Missing function*/";
        //        method = $"CreateVariableFactor({functionCode}, {variableFactor.min.ToCodeFormat()}, {variableFactor.max.ToCodeFormat()})";
        //    }
        //    else if(factor is FunctionFactor functionFactor)
        //    {
        //        var child = node.Childs.FirstOrDefault();
        //        string childName = child != null ? template.FindVariableName(child) ?? AddFactor(child, template, graphName) : "null /*Missing child*/";
        //        method = $"CreateFunctionFactor<{typeName}>({childName}){GenerateSetterCode(functionFactor, template)}";
        //    }
        //    else if(factor is FusionFactor fusionFactor)
        //    {
        //        var args = new List<string>();
        //        foreach(var child in node.Childs)
        //        {
        //            var childName = template.FindVariableName(child) ?? AddFactor(child, template, graphName);
        //            if(childName != null) args.Add(childName);
        //        }
        //        method = $"CreateFusionFactor<{typeName}>({args.Join()})";

        //        if(fusionFactor is WeightedFusionFactor weightedFusionFactor)
        //        {
        //            var weightArgs = weightedFusionFactor.Weights.Select(w => w.ToCodeFormat());
        //            if(weightArgs.Count() > 0) method += $".SetWeights({weightArgs.Join()})";
        //        }
        //    }
        //    return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{method}");
        //}

        //string AddUtilityElement(NodeAsset node, ScriptTemplate template, string graphName)
        //{
        //    UtilitySelectableNode selectableNode = node.Node as UtilitySelectableNode;
        //    var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : selectableNode.TypeName().ToLower();
        //    string typeName = selectableNode.TypeName();

        //    var method = string.Empty;

        //    var args = new List<string>();            

        //    if (string.IsNullOrEmpty(nodeName)) nodeName = selectableNode.TypeName().ToLower();

        //    if (selectableNode is UtilityAction action)
        //    {
        //        args.Add(template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null /*Error*/");

        //        if (action.Action != null) args.Add(GenerateActionCode(action.Action, template));
        //        if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
        //        if (selectableNode.IsRoot) args.Add("root: true");
        //        method = $"CreateUtilityAction({args.Join()})";
        //    }
        //    else if (selectableNode is UtilityExitNode exitNode)
        //    {
        //        args.Add(template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null /*Error*/");

        //        if (selectableNode.IsRoot) args.Add("root: true");
        //        method = $"CreateUtilityExitNode({args.Join()})";
        //    }
        //    else if (selectableNode is UtilityBucket bucket)
        //    {
        //        args.Add($"{bucket.IsRoot.ToCodeFormat()}");
        //        args.Add($"{bucket.UtilityThreshold.ToCodeFormat()}");
        //        args.Add($"{bucket.Inertia.ToCodeFormat()}");
        //        args.Add($"{bucket.BucketThreshold.ToCodeFormat()}");                

        //        node.Childs.ForEach(child =>
        //        {
        //            string childName = template.FindVariableName(child) ?? AddUtilityElement(child, template, graphName);
        //            if (childName != null) args.Add(childName);
        //        });
        //        method = $"CreateUtilityBucket({args.Join()})";
        //    }
        //    return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{method}");
        //}      

        //public override string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate, string graphName)
        //{
        //    if (graphAsset.Graph is UtilitySystem utilitySystem)
        //    {
        //        scriptTemplate.AddUsingDirective(typeof(UtilitySystem).Namespace);

        //        return scriptTemplate.AddVariableInstantiationLine(utilitySystem.TypeName(), graphName, graphAsset,
        //            utilitySystem.Inertia.ToCodeFormat(), utilitySystem.UtilityThreshold.ToCodeFormat());
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            throw new NotImplementedException();
        }

        #region ------------------- Rendering -------------------

        protected override List<Type> MainTypes => new List<Type>
        {
            typeof(UtilityAction),
            typeof(UtilityBucket),
            typeof(UtilityExitNode),
            typeof(FusionFactor),
            typeof(FunctionFactor),
            typeof(VariableFactor)
        };
        protected override List<Type> ExcludedTypes => new List<Type>
        {
            typeof(UtilitySystems.UtilityAction),
            typeof(UtilitySystems.PointedFunction),
            typeof(UtilitySystems.CustomFunction),
            typeof(UtilitySystems.VariableFactor)
        };

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {            
        }

        protected override NodeView GetLayout(NodeAsset asset, BehaviourGraphView graphView) => new LayeredNodeView(asset, graphView);

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {            
        }
        protected override void SetUpDetails(NodeView nodeView)
        {
            var node = nodeView.Node.Node;
            if (node is VariableFactor) return;
            else
            {
                nodeView.IconElement.Enable();
                if(node is Factor) nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().Split().First().ToUpper()));
                else nodeView.IconElement.Add(new Label(node.TypeName().CamelCaseToSpaced().ToUpper()));
            }
        }

        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            return change;
        }

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
        }


        #endregion
    }
}

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

using CustomFunction = BehaviourAPI.Unity.Framework.Adaptations.CustomFunction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(UtilitySystem))]
    public class UtilitySystemAdapter : GraphAdapter
    {
        public override void ConvertAssetToCode(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            var graph = graphAsset.Graph;
            scriptTemplate.AddLine($"// UtilitySystem - {graphName}:");

            var utilityElements = graphAsset.Nodes.FindAll(n => n.Node is UtilitySelectableNode);
            var factors = graphAsset.Nodes.FindAll(n => n.Node is Factor);

            foreach(var factor in factors)
            {
                if (scriptTemplate.FindVariableName(factor) == null)
                {
                    AddFactor(factor, scriptTemplate, graphName);
                }                    
            }

            scriptTemplate.AddLine("");

            foreach (var utilityElement in utilityElements)
            {
                if (scriptTemplate.FindVariableName(utilityElement) == null)
                {
                    AddUtilityElement(utilityElement, scriptTemplate, graphName);
                }
            }

            var defaultElement = utilityElements.FirstOrDefault();
            if (defaultElement != null)
            {
                var defaultElementName = scriptTemplate.FindVariableName(defaultElement);
                if (!string.IsNullOrEmpty(defaultElementName))
                {
                    scriptTemplate.AddLine($"{graphName}.SetDefaultUtilityElement({defaultElementName});");
                }
            }
        }

        string AddFactor(NodeAsset node, ScriptTemplate template, string graphName)
        {
            Factor factor = node.Node as Factor;
            var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : factor.TypeName().ToLower();
            string typeName = factor.TypeName();

            var method = string.Empty;

            if(factor is VariableFactor variableFactor)
            {
                var functionCode = GenerateSerializedMethodCode(variableFactor.variableFunction, template) ?? "() => 0f /*Missing function*/";
                method = $"CreateVariableFactor({functionCode}, {variableFactor.min.ToCodeFormat()}, {variableFactor.max.ToCodeFormat()})";
            }
            else if(factor is FunctionFactor functionFactor)
            {
                var child = node.Childs.FirstOrDefault();
                string childName = child != null ? template.FindVariableName(child) ?? AddFactor(child, template, graphName) : "null /*Missing child*/";
                method = $"CreateFunctionFactor<{typeName}>({childName}){AddFunctionFactorProperties(functionFactor, template)}";
            }
            else if(factor is FusionFactor fusionFactor)
            {
                var args = new List<string>();
                foreach(var child in node.Childs)
                {
                    var childName = template.FindVariableName(child) ?? AddFactor(child, template, graphName);
                    if(childName != null) args.Add(childName);
                }
                method = $"CreateFusionFactor<{typeName}>({args})";

                if(fusionFactor is WeightedFusionFactor weightedFusionFactor)
                {
                    var weightArgs = weightedFusionFactor.Weights.Select(w => w.ToCodeFormat());
                    if(weightArgs.Count() > 0) method += $".SetWeights({weightArgs.Join()})";
                }
            }
            return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{method}");
        }

        string AddUtilityElement(NodeAsset node, ScriptTemplate template, string graphName)
        {
            UtilitySelectableNode selectableNode = node.Node as UtilitySelectableNode;
            var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : selectableNode.TypeName().ToLower();
            string typeName = selectableNode.TypeName();

            var method = string.Empty;

            var args = new List<string>();
            args.Add(template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null /*Error*/");

            if (string.IsNullOrEmpty(nodeName)) nodeName = selectableNode.TypeName().ToLower();

            if (selectableNode is UtilityAction action)
            {
                if (action.Action != null) args.Add(GenerateActionCode(action.Action, template));
                if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
                if (selectableNode.IsRoot) args.Add("root: true");
                method = $"CreateUtilityAction({args.Join()})";
            }
            else if (selectableNode is UtilityExitNode exitNode)
            {
                if (selectableNode.IsRoot) args.Add("root: true");
                method = $"CreateUtilityExitNode({args.Join()})";
            }
            else if (selectableNode is UtilityBucket bucket)
            {
                args.Add($"utilityThreshold: {bucket.UtilityThreshold.ToCodeFormat()}");
                args.Add($"inertia: {bucket.Inertia.ToCodeFormat()}");
                args.Add($"bucketThreshold: {bucket.BucketThreshold.ToCodeFormat()}");
                if (selectableNode.IsRoot) args.Add("root: true");

                node.Childs.ForEach(child =>
                {
                    string childName = template.FindVariableName(child) ?? AddUtilityElement(child, template, graphName);
                    if (childName != null) args.Add(childName);
                });
                method = $"CreateUtilityExitNode({args.Join()})";
            }
            return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{method}");
        }

        private string AddFunctionFactorProperties(FunctionFactor functionFactor, ScriptTemplate scriptTemplate)
        {
            if (functionFactor is LinearFunction linear)
            {
                return $".SetSlope({linear.Slope.ToCodeFormat()}).SetYIntercept({linear.YIntercept.ToCodeFormat()})";
            }
            else if (functionFactor is ExponentialFunction exponential)
            {
                return $".SetSetExponent({exponential.Exponent.ToCodeFormat()}).SetDespX({exponential.DespX.ToCodeFormat()}).SetDespY({exponential.DespY.ToCodeFormat()})";
            }
            else if (functionFactor is SigmoidFunction sigmoid)
            {
                return $".SetMidpoint({sigmoid.Midpoint.ToCodeFormat()}).SetGrownRate({sigmoid.GrownRate.ToCodeFormat()})";
            }
            else if (functionFactor is CustomFunction custom)
            {
                var functionCode = GenerateSerializedMethodCode(custom.function, scriptTemplate) ?? "null /*missing function*/";
                return $".SetFunction({functionCode})";
            }
            else if (functionFactor is CurveFunction curve)
            {
                var curvePropertyName = scriptTemplate.AddPropertyLine(nameof(AnimationCurve), "factorCurve", curve.curve);
                return $".SetCurve({curvePropertyName})";
            }
            else
            {
                return string.Empty;
            }
        }

        public override string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate, string graphName)
        {
            if (graphAsset.Graph is UtilitySystem utilitySystem)
            {
                scriptTemplate.AddUsingDirective(typeof(UtilitySystem).Namespace);
                scriptTemplate.AddUsingDirective($"{nameof(VariableFactor)} = {typeof(UtilitySystems.VariableFactor).FullName}");
                scriptTemplate.AddUsingDirective($"{nameof(CustomFunction)} = {typeof(UtilitySystems.CustomFunction).FullName}");

                return scriptTemplate.AddVariableInstantiationLine(utilitySystem.TypeName(), graphName, graphAsset,
                    utilitySystem.Inertia.ToCodeFormat(), utilitySystem.UtilityThreshold.ToCodeFormat());
            }
            else
            {
                return null;
            }
        }

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
            typeof(UtilitySystems.CustomFunction),
            typeof(UtilitySystems.VariableFactor)
        };

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {            
        }

        protected override string GetNodeLayoutPath(NodeAsset node)
        {
            return BehaviourAPISettings.instance.EditorElementPath + "/Nodes/DAG Node.uxml";
            //return AssetDatabase.GetAssetPath(VisualSettings.GetOrCreateSettings().NodeLayout);
        }

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {            
        }

        protected override void SetUpPortsAndDetails(NodeView nodeView)
        {
            if (nodeView.Node.Node.MaxInputConnections != 0)
            {
                var port = CreatePort(nodeView, Direction.Input, PortOrientation.Right);
            }
            else
            {
                nodeView.inputContainer.style.display = DisplayStyle.None;
            }

            if (nodeView.Node.Node.MaxOutputConnections != 0)
            {
                var port = CreatePort(nodeView, Direction.Output, PortOrientation.Left);
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;
        }

        protected override void DecoratePort(PortView port)
        { 
            var bg = new VisualElement();
            bg.style.position = Position.Absolute;
            bg.style.top = 0; bg.style.left = 0; bg.style.bottom = 0; bg.style.right = 0;
            port.Add(bg);
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

using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UtilitySystems;
using BehaviourAPI.UtilitySystems.UtilityElements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CustomFunction = BehaviourAPI.Unity.Runtime.CustomFunction;
using VariableFactor = BehaviourAPI.Unity.Runtime.VariableFactor;

namespace BehaviourAPI.Unity.Editor
{
    [CustomConverter(typeof(UtilitySystem))]
    public class UtilitySystemConverter : GraphConverter
    {
        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(UtilitySystem)) return null;

            return GraphAsset.Create("new graph", typeof(UtilitySystem));
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(UtilitySystem)) return;

            graphName = scriptTemplate.FindVariableName(asset);

            var graph = asset.Graph;

            scriptTemplate.AddLine($"// UtilitySystem - {graphName}:");

            var utilityElements = asset.Nodes.FindAll(n => n.Node is UtilitySelectableNode);
            var factors = asset.Nodes.FindAll(n => n.Node is Factor);


            factors.ForEach(factor =>
            {
                if (scriptTemplate.FindVariableName(factor) == null)
                    AddFactor(factor, scriptTemplate);
            });

            scriptTemplate.AddLine("");
            utilityElements.ForEach(utilityElement =>
            {
                if (scriptTemplate.FindVariableName(utilityElement) == null)
                    AddUtilityElement(utilityElement, scriptTemplate);
            });

            var defaultElement = utilityElements.FirstOrDefault();

            if (defaultElement != null)
            {
                var defaultElementName = scriptTemplate.FindVariableName(defaultElement);
                if (!string.IsNullOrEmpty(defaultElementName)) scriptTemplate.AddLine($"{graphName}.SetDefaultUtilityElement({defaultElementName});");
            }
        }

        string AddFactor(NodeAsset node, ScriptTemplate template)
        {
            Factor factor = node.Node as Factor;
            var nodeName = node.Name;
            string typeName = factor.TypeName();

            if (string.IsNullOrEmpty(nodeName)) nodeName = factor.TypeName().ToLower();

            if (factor is VariableFactor variableFactor)
            {
                string functionCode;
                if (variableFactor.variableFunction != null && variableFactor.variableFunction.component != null && string.IsNullOrEmpty(variableFactor.variableFunction.methodName))
                {
                    var componentName = template.AddPropertyLine(variableFactor.variableFunction.component.TypeName(), variableFactor.variableFunction.component.TypeName().ToLower(), variableFactor.variableFunction.component);
                    functionCode = $"{componentName}.{variableFactor.variableFunction.methodName}";
                }
                else functionCode = "() => 0f /*ERROR*/";
                return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateVariableFactor({functionCode}, {variableFactor.min}, {variableFactor.max})");
            }
            else if (factor is FunctionFactor functionFactor)
            {
                var child = node.Childs.FirstOrDefault();

                string childName = null;
                if (child != null)
                {
                    childName = template.FindVariableName(child);
                    if (childName == null) childName = AddFactor(child, template);
                }
                return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateFunctionFactor<{typeName}>({childName ?? "null /*ERROR*/"})");
            }
            else if (factor is FusionFactor fusionFactor)
            {
                var args = new List<string>();
                node.Childs.ForEach(child =>
                {
                    string childName = template.FindVariableName(child);
                    if (childName == null) childName = AddFactor(child, template);
                    if (childName != null) args.Add(childName);
                });

                var setter = "";
                if(fusionFactor is WeightedFusionFactor weighted)
                {
                    var weightArgs = weighted.Weights.Select(w => w.ToCodeFormat());
                    setter = $".SetWeights({string.Join(", ", weightArgs)})";
                }

                return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateFusionFactor<{typeName}>({string.Join(", ", args)}){setter}");
            }
            else
                return null;
        }

        string AddUtilityElement(NodeAsset node, ScriptTemplate template)
        {
            UtilitySelectableNode selectableNode = node.Node as UtilitySelectableNode;
            var nodeName = node.Name;
            string typeName = selectableNode.TypeName();
            var methodName = string.Empty;

            var args = new List<string>();
            args.Add(template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null /*Error*/");           

            if (string.IsNullOrEmpty(nodeName)) nodeName = selectableNode.TypeName().ToLower();

            if(selectableNode is UtilityAction action)
            {
                if (action.Action != null) args.Add(GetActionCode(action.Action, template));
                if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
                if (selectableNode.IsRoot) args.Add("root: true");
                methodName = "CreateUtilityAction";                
            }
            else if(selectableNode is UtilityExitNode exitNode)
            {
                if (selectableNode.IsRoot) args.Add("root: true");
                methodName = "CreateUtilityExitNode";
            }
            else if(selectableNode is UtilityBucket bucket)
            {
                args.Add($"utilityThreshold: {bucket.UtilityThreshold.ToCodeFormat()}");
                args.Add($"inertia: {bucket.Inertia.ToCodeFormat()}");
                args.Add($"bucketThreshold: {bucket.BucketThreshold.ToCodeFormat()}");
                if (selectableNode.IsRoot) args.Add("root: true");

                node.Childs.ForEach(child =>
                {
                    string childName = template.FindVariableName(child);
                    if (childName == null) childName = AddUtilityElement(child, template);
                    if (childName != null) args.Add(childName);
                });
                methodName = "CreateUtilityBucket";
            }

            if (!string.IsNullOrEmpty(methodName)) return template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{methodName}({string.Join(", ", args)})");
            else return null;
        }

        public override string AddCreateGraphLine(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            scriptTemplate.AddUsingDirective(typeof(UtilitySystem).Namespace);
            scriptTemplate.AddUsingDirective($"{nameof(VariableFactor)} = {typeof(UtilitySystems.VariableFactor).FullName}");
            scriptTemplate.AddUsingDirective($"{nameof(CustomFunction)} = {typeof(UtilitySystems.CustomFunction).FullName}");

            var utilitySystem = asset.Graph as UtilitySystem;
            return scriptTemplate.AddVariableInstantiationLine(asset.Graph.TypeName(), asset.Name, asset, utilitySystem.Inertia.ToCodeFormat(), utilitySystem.UtilityThreshold.ToCodeFormat());
        }

        private string AddFunctionFactorProperties(FunctionFactor functionFactor, ScriptTemplate scriptTemplate)
        {
            if (functionFactor is LinearFunction linear)
            {
                return $".SetSlope({linear.slope.ToCodeFormat()}).SetYIntercept({linear.yIntercept.ToCodeFormat()})";
            }
            else if (functionFactor is ExponentialFunction exponential)
            {
                return $".SetSetExponent({exponential.Exp.ToCodeFormat()}).SetDespX({exponential.DespX.ToCodeFormat()}).SetDespY({exponential.DespY.ToCodeFormat()})";
            }
            else if (functionFactor is SigmoidFunction sigmoid)
            {
                return $".SetMidpoint({sigmoid.midpoint.ToCodeFormat()}).SetGrownRate({sigmoid.grownRate.ToCodeFormat()})";
            }
            else if(functionFactor is CustomFunction custom)
            {
                if(custom.function != null && custom.function.component != null && string.IsNullOrEmpty(custom.function.methodName))
                {
                    var componentName = scriptTemplate.AddPropertyLine(custom.function.component.TypeName(), custom.function.component.TypeName().ToLower(), custom.function.component);
                    return $".SetFunction({componentName}.{custom.function.methodName})";
                }
                else return string.Empty;
            }
            else if(functionFactor is CurveFunction curve)
            {
                var curvePropertyName = scriptTemplate.AddPropertyLine(nameof(AnimationCurve), "factorCurve", curve.curve);
                return $".SetCurve({curvePropertyName})";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
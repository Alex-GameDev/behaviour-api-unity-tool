using UnityEngine;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    using Framework;
    using UtilitySystems;

    using UtilityAction = Framework.Adaptations.UtilityAction;
    using VariableFactor = Framework.Adaptations.VariableFactor;


    [CustomGraphCodeGenerator(typeof(UtilitySystem))]
    public class UtilitySystemCodeGenerator : GraphCodeGenerator
    {
        private static readonly string k_VariableFactorMethod = "CreateVariable";
        private static readonly string k_ConstantFactorMethod = "CreateConstant";
        private static readonly string k_CurveFactorMethod = "CreateCurve";
        private static readonly string k_FusionFactorMethod = "CreateFusion";

        private static readonly string k_UtilityActionMethod = "CreateAction";
        private static readonly string k_UtilityExitNodeMethod = "CreateExitNode";
        private static readonly string k_UtilityBucketMethod = "CreateBucket";

        public override void GenerateGraphDeclaration(GraphData graphData, CodeTemplate template)
        {
            GraphIdentifier = template.GetSystemElementIdentifier(graphData.id);
            var type = graphData.graph.GetType();
            UtilitySystem us = graphData.graph as UtilitySystem;

            var graphCreationExpression = new CodeObjectCreationExpression(us.GetType());
            graphCreationExpression.Add(new CodeCustomExpression(us.Inertia.ToCodeFormat()));

            var graphStatement = new CodeVariableDeclarationStatement(type, GraphIdentifier);
            graphStatement.RightExpression = graphCreationExpression;

            template.AddNamespace("BehaviourAPI.UtilitySystems");
            template.AddGraphCreationStatement(graphStatement);

            foreach (var data in graphData.nodes)
            {
                GenerateNodeCode(data, template);
            }
        }

        private void GenerateNodeCode(NodeData data, CodeTemplate template)
        {
            if (data == null) return;
            if (IsGenerated(data.id)) return;

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement(data.node.GetType(), template.GetSystemElementIdentifier(data.id));
            switch (data.node)
            {
                case VariableFactor variableFactor:
                    nodeDeclaration.RightExpression = GenerateVariableFactorCode(data, variableFactor, template); break;
                case ConstantFactor constantFactor:
                    nodeDeclaration.RightExpression = GenerateConstantFactorCode(data, constantFactor, template); break;
                case FusionFactor fusionFactor:
                    nodeDeclaration.RightExpression = GenerateFusionFactorCode(data, fusionFactor, template); break;
                case CurveFactor curveFactor:
                    nodeDeclaration.RightExpression = GenerateCurveFactorCode(data, curveFactor, template); break;
                case UtilityAction utilityAction:
                    nodeDeclaration.RightExpression = GenerateutilityActionCode(data, utilityAction, template); break;
                case UtilityExitNode utilityExitNode:
                    nodeDeclaration.RightExpression = GenerateutilityExitNodeCode(data, utilityExitNode, template); break;
                case UtilityBucket utilityBucket:
                    nodeDeclaration.RightExpression = GenerateutilityBucketCode(data, utilityBucket, template); break;
            }
            template.AddStatement(nodeDeclaration);
            MarkGenerated(data.id);
        }

        private CodeNodeCreationMethodExpression GenerateutilityBucketCode(NodeData data, UtilityBucket utilityBucket, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_UtilityBucketMethod);

            initMethod.Add(new CodeCustomExpression(utilityBucket.Inertia.ToCodeFormat()));
            initMethod.Add(new CodeCustomExpression(utilityBucket.BucketThreshold.ToCodeFormat()));
            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateutilityExitNodeCode(NodeData data, UtilityExitNode utilityExitNode, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_UtilityExitNodeMethod);

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(GetChildExpression(data.childIds[0], template));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }

            initMethod.Add(new CodeCustomExpression(utilityExitNode.ExitStatus.ToCodeFormat()));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(GetChildExpression(data.parentIds[0], template));
            }

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateutilityActionCode(NodeData data, UtilityAction utilityAction, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_UtilityActionMethod);

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(GetChildExpression(data.childIds[0], template));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }

            initMethod.Add(template.GetActionExpression(utilityAction.ActionReference, template.GetSystemElementIdentifier(data.id) + "_action"));
            initMethod.Add(new CodeCustomExpression(utilityAction.FinishSystemOnComplete.ToCodeFormat()));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(GetChildExpression(data.parentIds[0], template));
            }

            return initMethod;

        }

        private CodeNodeCreationMethodExpression GenerateCurveFactorCode(NodeData data, CurveFactor curveFactor, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_CurveFactorMethod + "<" + data.node.TypeName() + ">");

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(GetChildExpression(data.childIds[0], template));
            }
            else
            {
                initMethod.Add(new CodeCustomExpression("null"));
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
            }
            GenerateUtilityNodeProperties(data.node as UtilityNode, template.GetSystemElementIdentifier(data.id), template);
            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateFusionFactorCode(NodeData data, FusionFactor fusionFactor, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_FusionFactorMethod + "<" + data.node.TypeName() + ">");

            for (int i = 0; i < data.childIds.Count; i++)
            {
                GenerateNodeCode(GetNodeById(data.childIds[i]), template);
                initMethod.Add(GetChildExpression(data.childIds[i], template));
            }

            GenerateUtilityNodeProperties(data.node as UtilityNode, template.GetSystemElementIdentifier(data.id), template);
            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateConstantFactorCode(NodeData data, ConstantFactor constantFactor, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_ConstantFactorMethod);

            initMethod.Add(new CodeCustomExpression(constantFactor.value.ToCodeFormat()));

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateVariableFactorCode(NodeData data, VariableFactor variableFactor, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_VariableFactorMethod);

            initMethod.Add(template.GenerateMethodCodeExpression(variableFactor.variableFunction, null, typeof(float)));
            initMethod.Add(new CodeCustomExpression(variableFactor.min.ToCodeFormat()));
            initMethod.Add(new CodeCustomExpression(variableFactor.max.ToCodeFormat()));

            return initMethod;
        }
        private void GenerateUtilityNodeProperties(UtilityNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.Name == "PullingEnabled") continue;

                template.AddPropertyStatement(field.GetValue(obj), identifier, field.Name);
            }
        }

    }
}

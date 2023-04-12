using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
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
            GraphIdentificator = template.GetSystemElementIdentificator(graphData.id);
            var type = graphData.graph.GetType();
            UtilitySystem us = graphData.graph as UtilitySystem;

            var graphCreationExpression = new CodeObjectCreationExpression(us.GetType());
            graphCreationExpression.Add(new CodeSimpleExpression(us.Inertia.ToCodeFormat()));

            var graphStatement = new CodeVariableDeclarationStatement()
            {
                Type = type,
                Identificator = GraphIdentificator,
                RightExpression = graphCreationExpression
            };

            template.AddNamespace("BehaviourAPI.UtilitySystems");
            template.AddStatement(graphStatement, true);

            foreach (var data in graphData.nodes)
            {
                GenerateNodeCode(data, template);
            }
        }

        private void GenerateNodeCode(NodeData nodeData, CodeTemplate template)
        {
            if (nodeData == null) return;
            if (IsGenerated(nodeData.id)) return;

            switch (nodeData.node)
            {
                case VariableFactor variableFactor: GenerateVariableFactorCode(nodeData, variableFactor, template); break;
                case ConstantFactor constantFactor: GenerateConstantFactorCode(nodeData, constantFactor, template); break;
                case FusionFactor fusionFactor: GenerateFusionFactorCode(nodeData, fusionFactor, template); break;
                case CurveFactor curveFactor: GenerateCurveFactorCode(nodeData, curveFactor, template); break;
                case UtilityAction utilityAction: GenerateutilityActionCode(nodeData, utilityAction, template); break;
                case UtilityExitNode utilityExitNode: GenerateutilityExitNodeCode(nodeData, utilityExitNode, template); break;
                case UtilityBucket utilityBucket: GenerateutilityBucketCode(nodeData, utilityBucket, template); break;
            }
            MarkGenerated(nodeData.id);
        }

        private void GenerateutilityBucketCode(NodeData data, UtilityBucket utilityBucket, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_UtilityBucketMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.CreateGenericExpression(utilityBucket.Inertia.ToCodeFormat()));
            initMethod.Add(template.CreateGenericExpression(utilityBucket.BucketThreshold.ToCodeFormat()));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = utilityBucket.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateutilityExitNodeCode(NodeData data, UtilityExitNode utilityExitNode, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_UtilityExitNodeMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }

            initMethod.Add(template.CreateReferencedElementExpression(utilityExitNode.ExitStatus.ToCodeFormat()));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = utilityExitNode.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateutilityActionCode(NodeData data, UtilityAction utilityAction, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_UtilityActionMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }

            initMethod.Add(template.GetActionExpression(utilityAction.ActionReference));
            initMethod.Add(template.CreateGenericExpression(utilityAction.FinishSystemOnComplete.ToCodeFormat()));

            if (data.parentIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.parentIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.parentIds[0]));
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = utilityAction.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateCurveFactorCode(NodeData data, CurveFactor curveFactor, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_CurveFactorMethod + "<" + curveFactor.TypeName() + ">")
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.childIds.Count == 1)
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }
            else
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = curveFactor.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
            GenerateUtilityNodeProperties(curveFactor, nodeDeclaration.Identificator, template);
        }

        private void GenerateFusionFactorCode(NodeData data, FusionFactor fusionFactor, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_FusionFactorMethod
                + "<" + fusionFactor.TypeName() + ">")
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            for (int i = 0; i < data.childIds.Count; i++)
            {
                GenerateNodeCode(GetNodeById(data.childIds[i]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[i]));
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = fusionFactor.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);

            GenerateUtilityNodeProperties(fusionFactor, nodeDeclaration.Identificator, template);
        }

        private void GenerateConstantFactorCode(NodeData data, ConstantFactor constantFactor, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_ConstantFactorMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.CreateGenericExpression(constantFactor.value.ToCodeFormat()));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = constantFactor.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateVariableFactorCode(NodeData data, VariableFactor variableFactor, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_VariableFactorMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.GenerateMethodCodeExpression(variableFactor.variableFunction, null, typeof(float)));
            initMethod.Add(template.CreateGenericExpression(variableFactor.min.ToCodeFormat()));
            initMethod.Add(template.CreateGenericExpression(variableFactor.max.ToCodeFormat()));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = variableFactor.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }
        private void GenerateUtilityNodeProperties(UtilityNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.Name == "PullingEnabled") continue;

                var statement = new CodeVariableReassignationStatement();
                statement.LeftExpression = new CodeMethodReferenceExpression(identifier, field.Name);

                if (typeof(Action).IsAssignableFrom(field.FieldType))
                {
                    statement.RightExpression = template.GetActionExpression((Action)field.GetValue(obj));
                }
                else if (typeof(Perception).IsAssignableFrom(field.FieldType))
                {
                    statement.RightExpression = template.GetPerceptionExpression((Perception)field.GetValue(obj));
                }
                else
                {
                    statement.RightExpression = template.CreatePropertyExpression(field.GetValue(obj));
                }
                template.AddStatement(statement);
            }
        }

    }
}

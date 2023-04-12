using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
    using BehaviourTrees;
    using Framework;
    using LeafNode = Framework.Adaptations.LeafNode;

    [CustomGraphCodeGenerator(typeof(BehaviourTree))]
    public class BehaviourTreeCodeGenerator : GraphCodeGenerator
    {
        private static readonly string k_DecoratorMethod = "CreateDecorator";
        private static readonly string k_CompositeMethod = "CreateComposite";
        private static readonly string k_LeafMethod = "CreateLeafNode";

        public override void GenerateGraphDeclaration(GraphData graphData, CodeTemplate template)
        {
            GraphIdentificator = template.GetSystemElementIdentificator(graphData.id);
            var type = graphData.graph.GetType();
            var graphStatement = new CodeVariableDeclarationStatement()
            {
                Type = type,
                Identificator = GraphIdentificator,
                RightExpression = new CodeObjectCreationExpression(type)
            };


            template.AddNamespace("BehaviourAPI.BehaviourTrees");
            template.AddStatement(graphStatement, true);

            foreach (NodeData nodeData in graphData.nodes)
            {
                GenerateNodeCode(nodeData, template);
            }
        }

        private void GenerateNodeCode(NodeData nodeData, CodeTemplate template)
        {
            if (nodeData == null) return;
            if (IsGenerated(nodeData.id)) return;

            switch (nodeData.node)
            {
                case LeafNode leafNode: GenerateLeafNodeCode(leafNode, nodeData, template); break;
                case DecoratorNode decoratorNode: GenerateDecoratorCode(decoratorNode, nodeData, template); break;
                case CompositeNode compositeNode: GenerateCompositeCode(compositeNode, nodeData, template); break;
            }
            MarkGenerated(nodeData.id);
        }

        private void GenerateLeafNodeCode(LeafNode leafNode, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_LeafMethod)
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(template.GetActionExpression(leafNode.ActionReference));

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = typeof(LeafNode),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
        }

        private void GenerateDecoratorCode(DecoratorNode decoratorNode, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_DecoratorMethod + "<" + decoratorNode.TypeName() + ">")
            };

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            if (data.childIds.Count != 1)
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeSimpleExpression("null /* missing node */"));
            }
            else
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);

                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[0]));
            }

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = decoratorNode.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
            GenerateDecoratorProperties(decoratorNode, nodeDeclaration.Identificator, template);
        }


        private void GenerateCompositeCode(CompositeNode compositeNode, NodeData data, CodeTemplate template)
        {
            CodeMethodInvocationExpression initMethod = new CodeMethodInvocationExpression()
            {
                methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentificator, k_CompositeMethod + "<" + compositeNode.TypeName() + ">")
            };

            var randomArg = template.CreateGenericExpression(compositeNode.IsRandomized.ToCodeFormat());

            if (!string.IsNullOrWhiteSpace(data.name)) initMethod.Add(template.CreateGenericExpression("\"" + data.name + "\""));

            initMethod.Add(randomArg);

            for (int i = 0; i < data.childIds.Count; i++)
            {
                GenerateNodeCode(GetNodeById(data.childIds[i]), template);
                initMethod.Add(template.CreateReferencedElementExpression(data.childIds[i]));
            }



            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement()
            {
                Type = compositeNode.GetType(),
                Identificator = template.GetSystemElementIdentificator(data.id),
                RightExpression = initMethod
            };

            template.AddStatement(nodeDeclaration);
            GenerateCompositeProperties(compositeNode, nodeDeclaration.Identificator, template);
        }

        private void GenerateDecoratorProperties(DecoratorNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
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

        private void GenerateCompositeProperties(CompositeNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.Name == "IsRandomized") continue;

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

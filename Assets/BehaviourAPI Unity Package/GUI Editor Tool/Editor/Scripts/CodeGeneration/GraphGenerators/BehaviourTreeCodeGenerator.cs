using UnityEngine;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
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
            GraphIdentifier = template.GetSystemElementIdentifier(graphData.id);
            var type = graphData.graph.GetType();
            var graphStatement = new CodeVariableDeclarationStatement(type, GraphIdentifier);
            graphStatement.RightExpression = new CodeObjectCreationExpression(type);

            template.AddNamespace("BehaviourAPI.BehaviourTrees");
            template.AddGraphCreationStatement(graphStatement);

            foreach (NodeData nodeData in graphData.nodes)
            {
                GenerateNodeCode(nodeData, template);
            }
        }

        private void GenerateNodeCode(NodeData data, CodeTemplate template)
        {
            if (data == null) return;
            if (IsGenerated(data.id)) return;

            CodeVariableDeclarationStatement nodeDeclaration = new CodeVariableDeclarationStatement(data.node.GetType(), template.GetSystemElementIdentifier(data.id));

            switch (data.node)
            {
                case LeafNode leafNode:
                    nodeDeclaration.RightExpression = GenerateLeafNodeCode(leafNode, data, template); break;
                case DecoratorNode decoratorNode:
                    nodeDeclaration.RightExpression = GenerateDecoratorCode(decoratorNode, data, template); break;
                case CompositeNode compositeNode:
                    nodeDeclaration.RightExpression = GenerateCompositeCode(compositeNode, data, template); break;
            }
            template.AddStatement(nodeDeclaration);

            MarkGenerated(data.id);
        }

        private CodeNodeCreationMethodExpression GenerateLeafNodeCode(LeafNode leafNode, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_LeafMethod);

            initMethod.Add(template.GetActionExpression(leafNode.ActionReference, template.GetSystemElementIdentifier(data.id) + "_action"));

            return initMethod;
        }

        private CodeNodeCreationMethodExpression GenerateDecoratorCode(DecoratorNode decoratorNode, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_DecoratorMethod + "<" + decoratorNode.TypeName() + ">");

            if (data.childIds.Count != 1)
            {
                Debug.LogWarning("CodeGenError: The number of children is wrong.");
                initMethod.Add(new CodeCustomExpression("null /* missing node */"));
            }
            else
            {
                GenerateNodeCode(GetNodeById(data.childIds[0]), template);
                initMethod.Add(GetChildExpression(data.childIds[0], template));
            }

            GenerateDecoratorProperties(decoratorNode, template.GetSystemElementIdentifier(data.id), template);
            return initMethod;
        }


        private CodeNodeCreationMethodExpression GenerateCompositeCode(CompositeNode compositeNode, NodeData data, CodeTemplate template)
        {
            CodeNodeCreationMethodExpression initMethod = new CodeNodeCreationMethodExpression();
            initMethod.nodeName = data.name;
            initMethod.methodReferenceExpression = new CodeMethodReferenceExpression(GraphIdentifier, k_CompositeMethod + "<" + compositeNode.TypeName() + ">");

            initMethod.Add(new CodeCustomExpression(compositeNode.IsRandomized.ToCodeFormat()));

            for (int i = 0; i < data.childIds.Count; i++)
            {
                GenerateNodeCode(GetNodeById(data.childIds[i]), template);
                initMethod.Add(GetChildExpression(data.childIds[0], template));
            }

            GenerateCompositeProperties(compositeNode, template.GetSystemElementIdentifier(data.id), template);

            return initMethod;
        }

        private void GenerateDecoratorProperties(DecoratorNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                template.AddPropertyStatement(field.GetValue(obj), identifier, field.Name);
            }
        }

        private void GenerateCompositeProperties(CompositeNode obj, string identifier, CodeTemplate template)
        {
            var type = obj.GetType();
            var fields = type.GetFields();

            foreach (var field in fields)
            {
                if (field.Name == "IsRandomized") continue;

                template.AddPropertyStatement(field.GetValue(obj), identifier, field.Name);
            }
        }
    }
}

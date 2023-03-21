using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using Microsoft.CSharp;
using UnityEngine;

// Opciones:
// Nombres en nodos
// Añadir linea Registergraph()
// Crear cada grafo en un submétodo

namespace BehaviourAPI.Unity.Editor
{
    public class RunnerCodeTemplate
    {
        CodeNamespace m_namespace;
        CodeTypeDeclaration m_classDeclaration;
        CodeMemberMethod m_mainMemberMethod;
        CodeMemberMethod m_awakeMemberMethod;
        Dictionary<string, CodeTypeMember> memberMap;

        Dictionary<Type, CodeFieldReferenceExpression> m_componentMap;
        CodeStatement m_currentStatement;


        CodeVariableReferenceExpression m_currentGraphReference;

        CodeMethodInvokeExpression m_currentMethodInvokeExpression;

        #region SET UP

        private void GenerateBaseCode(string className)
        {
            CodeCompileUnit unit = new CodeCompileUnit();

            m_namespace = new CodeNamespace();
            unit.Namespaces.Add(m_namespace);

            m_classDeclaration = new CodeTypeDeclaration(className);
            m_namespace.Types.Add(m_classDeclaration);

            m_mainMemberMethod = AddCreateGraphMethod();
            m_classDeclaration.Members.Add(m_mainMemberMethod);

            StringBuilder generatedCode = new StringBuilder();
            StringWriter codeWriter = new StringWriter(generatedCode);
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            codeProvider.GenerateCodeFromCompileUnit(unit, codeWriter, options);

            string code = generatedCode.ToString();
        }

        //Añadir metodo awake solo si se accede a algun componente
        private CodeMemberMethod AddAwakeMethod()
        {
            CodeMemberMethod awakeMethod = new CodeMemberMethod();
            awakeMethod.Name = "OnAwake";
            awakeMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
            var baseCallMethod = new CodeMethodInvokeExpression(
                new CodeBaseReferenceExpression(), "OnAwake");
            awakeMethod.Statements.Add(baseCallMethod);
            m_classDeclaration.Members.Add(awakeMethod);
            return awakeMethod;
        }

        // Añadir método de createGraph
        private CodeMemberMethod AddCreateGraphMethod()
        {
            CodeMemberMethod createGraphMethod = new CodeMemberMethod();
            createGraphMethod.Name = "CreateGraph";
            createGraphMethod.Attributes = MemberAttributes.Family | MemberAttributes.Override;
            createGraphMethod.ReturnType = new CodeTypeReference(typeof(BehaviourAPI.Core.BehaviourGraph));
            return createGraphMethod;
        }

        #endregion

        // Añadir grafo
        public void AddGraphDeclaration(GraphData data)
        {
            throw new NotImplementedException();
        }

        // Create a class variable member
        private string CreateClassVariableReference(object obj, string parameterName, CodeExpression value = null)
        {
            CodeMemberField field = new CodeMemberField(new CodeTypeReference(obj.GetType().Name), parameterName);
            field.InitExpression = value;
            field.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(SerializeField).Name)));
            m_classDeclaration.Members.Add(field);
            return parameterName;
        }

        // Genera método en la clase para acción, percepción o factor
        public CodeMethodReferenceExpression CreateEmptyMethod(string methodName, Type returnType, IEnumerable<Type> argumentTypes)
        {
            CodeMemberMethod customMethod = new CodeMemberMethod();
            customMethod.Name = methodName;
            customMethod.Attributes = MemberAttributes.Private;
            customMethod.ReturnType = new CodeTypeReference(returnType.Name);

            CodeParameterDeclarationExpressionCollection parameters = new CodeParameterDeclarationExpressionCollection();
            int id = 1;
            foreach (Type type in argumentTypes)
            {
                CodeParameterDeclarationExpression parameterExpression = new CodeParameterDeclarationExpression(
                    new CodeTypeReference(type.Name), $"{type.Name}_{id}");
                parameters.Add(parameterExpression);
            }
            customMethod.Parameters.AddRange(parameters);
            customMethod.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(new CodeTypeReference(typeof(NotImplementedException)))));
            return new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), methodName);
        }

        #region Componentes

        // Devuelve la referencia a un componente del objeto y la crea si no existe (no crear si es transform)
        public CodeFieldReferenceExpression GetOrCreateComponentReference(Type componentType)
        {
            if (m_componentMap.TryGetValue(componentType, out CodeFieldReferenceExpression fieldExpression))
            {
                return fieldExpression;
            }
            else
            {
                return CreateComponentReference(componentType);
            }
        }

        // Añade una referencia a un componente del objeto
        private CodeFieldReferenceExpression CreateComponentReference(Type componentType)
        {
            string componentFieldName = $"m_{componentType}";
            CodeMemberField componentField = new CodeMemberField(new CodeTypeReference(componentType), componentFieldName);
            componentField.Attributes = MemberAttributes.Private;
            m_classDeclaration.Members.Add(componentField);
            return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), componentFieldName);
        }

        #endregion
        // Devuelve la referencia a una variable global de la clase y la crea si no existe
        public string GetOrCreateGlobalVariableReference()
        {
            throw new NotImplementedException();
        }

        // Comienza una linea de declaración de un nodo
        public string CreateNodeReference(Type nodeType, string nodeName)
        {
            CodeStatement nodeDeclarationStatement = new CodeVariableDeclarationStatement(new CodeTypeReference(nodeType), nodeName);
            m_mainMemberMethod.UserData.Add(nodeName, nodeName);
            return nodeName;
        }

        // Generate a valid identificator
        public string GetValidIdentificator(string baseIdentificator, CodeObject context)
        {
            baseIdentificator = baseIdentificator.ToValidIdentificatorName();
            string finalIdentificator = baseIdentificator;
            int id = 1;
            while (context.UserData.Contains(finalIdentificator))
            {
                finalIdentificator = baseIdentificator + "_" + id;
                id++;
            }
            return finalIdentificator;
        }

        //Añade un método de un grafo para crear un nodo despues de una variable
        public string CreateGraphMethodAssignation(string methodName)
        {
            throw new NotImplementedException();
        }

        public string CreateVariableReassignation(string variableName)
        {
            throw new NotImplementedException();
        }

        private void CreateActionCode(Core.Actions.Action action, string defaultName)
        {
            switch (action)
            {
                case CustomAction customAction:

                    break;
            }
        }

        /// <summary>
        /// Generate a reference to a method member of a component. If the component is not defined, a method will be 
        /// created in the generated class.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="returnType">The return type of</param>
        /// <param name="args"></param>
        /// <returns></returns>
        // private CodeExpression GenerateSerializedMethodCode(SerializedContextMethod method, Type returnType, params Type[] args)
        // {
        //     if (!string.IsNullOrWhiteSpace(method.componentName))
        //     {
        //         Type componentType = TypeUtilities.FindType(method.componentName);
        //         if (componentType != null)
        //         {
        //             MethodInfo methodInfo = componentType.GetMethod(method.methodName, BindingFlags.Public, null, args, null);
        //             if (methodInfo != null)
        //             {
        //                 CodeFieldReferenceExpression componentReference = GetOrCreateComponentReference(componentType);
        //                 return new CodeMethodReferenceExpression(componentReference, methodInfo.Name);
        //             }
        //             else
        //             {
        //                 return new CodeSnippetExpression($"/* missing method */ null");
        //             }
        //         }
        //     }
        //     CodeMethodReferenceExpression emptyMethodExpression = CreateEmptyMethod(method.methodName, returnType, args);
        //     return emptyMethodExpression;
        // }

        /// <summary>
        /// Crea un nuevo parámetro para un método o asignación de variable.
        /// </summary>
        private CodeExpression GetCodeExpression(object obj, string parameterName = "")
        {
            Type objectType = obj.GetType();

            if (objectType.IsValueType)
            {
                if (objectType.IsPrimitive || objectType == typeof(string) || objectType.IsEnum)
                {
                    return new CodePrimitiveExpression(obj);
                }
                else if (objectType.IsEnum)
                {
                    return new CodePrimitiveExpression(obj);
                }
                else if (obj is LayerMask mask)
                {
                    string varName = CreateClassVariableReference(obj, parameterName, new CodePrimitiveExpression(mask.value));
                    return new CodeArgumentReferenceExpression(varName);
                }
                else
                {
                    CodeObjectCreateExpression structExpression = new CodeObjectCreateExpression(new CodeTypeReference(objectType));
                    switch (obj)
                    {
                        case Vector2 v:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.y));
                            break;
                        case Vector3 v:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.z));
                            break;
                        case Vector4 v:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.z));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.w));
                            break;
                        case Vector2Int v:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.y));
                            break;
                        case Vector3Int v:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(v.z));
                            break;
                        case Quaternion q:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(q.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(q.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(q.z));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(q.w));
                            break;
                        case Matrix4x4 m:
                            structExpression.Parameters.Add(GetCodeExpression(m.GetColumn(0)));
                            structExpression.Parameters.Add(GetCodeExpression(m.GetColumn(1)));
                            structExpression.Parameters.Add(GetCodeExpression(m.GetColumn(2)));
                            structExpression.Parameters.Add(GetCodeExpression(m.GetColumn(3)));
                            break;
                        case Rect r:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.width));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.height));
                            break;
                        case RectInt r:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.x));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.y));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.width));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(r.height));
                            break;
                        case Color c:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.r));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.g));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.b));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.a));
                            break;
                        case Color32 c:
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.r));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.g));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.b));
                            structExpression.Parameters.Add(new CodePrimitiveExpression(c.a));
                            break;
                        default:
                            string varName = CreateClassVariableReference(obj, parameterName);
                            return new CodeArgumentReferenceExpression(varName);
                    }
                    return structExpression;
                }
            }
            else
            {
                string variableName = CreateClassVariableReference(obj, parameterName);
                return new CodeArgumentReferenceExpression(variableName);
            }
        }

        // private void foo(NodeData nodeData)
        // {
        //     var nodeDeclaration = new CodeVariableDeclarationStatement();
        //     nodeDeclaration.Type = new CodeTypeReference(nodeData.node.TypeName());
        //     nodeDeclaration.Name = nodeData.name;
        //     var methodExpression = new CodeMethodInvokeExpression(m_currentGraphReference, "CreateNode");
        //     nodeDeclaration.InitExpression = methodExpression;
        //     methodExpression.Parameters.Add(GetCodeExpression())

        //     template.StartVariableReasignation(nodeData, "property").SetValue()
        //     //methodExpression.Parameters.Add()
        // }

        // public void StartNodeCreation(NodeData nodeData, string methodName, bool isGeneric = false)
        // {
        //     var variableName = GetValidVariableName(nodeData.name);
        //     var nodeDeclarationStatement = new CodeVariableDeclarationStatement(
        //         new CodeTypeReference(nodeData.node.TypeName()), variableName);

        //     if (isGeneric) methodName += $"<{nodeData.node.TypeName()}>";
        //     var methodExpression = new CodeMethodInvokeExpression(m_currentGraphReference, methodName);
        //     m_currentMethodInvokeExpression = methodExpression;

        //     if (m_includeNodeNames) methodExpression.Parameters.Add(new CodePrimitiveExpression(nodeData.name));

        //     nodeDeclarationStatement.InitExpression = methodExpression;
        // }

        // public void CloseNodeCreation()
        // {
        //     m_mainMemberMethod.Statements.Add(m_currentMethodInvokeExpression);
        //     m_mainMemberMethod = null;
        // }

        // public void AddParameter(object obj, string defaultName)
        // {
        //     CodeExpression expression = GetCodeExpression(obj, defaultName);
        //     m_currentMethodInvokeExpression.Parameters.Add(expression);
        // }

        // public void AddActionParameter(Core.Actions.Action action, string defaultName)
        // {
        //     CodeExpression expression = GetActionExpression(action, defaultName);
        //     m_currentMethodInvokeExpression.Parameters.Add(expression);
        // }

        // public void AddPerceptionParameter(Core.Perceptions.Perception perception, string defaultName)
        // {
        //     CodeExpression expression = GetPerceptionExpression(perception, defaultName);
        //     m_currentMethodInvokeExpression.Parameters.Add(expression);
        // }

        // public void AddNodeParameter(Core.Node node, string defaultName)
        // {
        //     CodeExpression nodeExpression = m_nodeExpressionMap[node];
        //     m_currentMethodInvokeExpression.Parameters.Add(nodeExpression);
        // }

        // public void GenerateBTNodeCode(NodeData nodeData, RunnerCodeTemplate template)
        // {
        //     switch (nodeData.node)
        //     {
        //         case LeafNode leaf: GenerateCodeForLeafNode(nodeData, leaf, template); break;
        //         case BehaviourTrees.CompositeNode composite:
        //             StartNodeCreation(nodeData, "CreateDecorator", isGeneric: true);
        //             AddPrimitiveParameter(composite.IsRandomized);
        //             for (int i = 0; i < nodeData.childIds.Count; i++)
        //             {
        //                 NodeData data = FindNodeById(nodeData.childIds[i]);
        //                 if (data != null && !Exists(data))
        //                 {
        //                     GenerateBTNodeCode(data);
        //                 }
        //                 AddNodeParameter(data);
        //             }
        //             CloseNodeCreation();
        //             break;

        //         case BehaviourTrees.DecoratorNode decorator:
        //             StartNodeCreation(nodeData, "CreateComposite", isGeneric: true);
        //             if (nodeData.childIds.Count > 0)
        //             {
        //                 NodeData data = FindNodeById(nodeData.childIds[0]);
        //                 if (data != null && !Exists(data))
        //                 {
        //                     GenerateBTNodeCode(data);
        //                 }
        //                 AddNodeParameter(data);
        //             }
        //             else
        //             {
        //                 AddNodeParameter(null);
        //             }
        //             CloseNodeCreation();
        //             // Add property code
        //             break;

        //         default:
        //             break;
        //     }
        // }

        // private void GenerateCodeForLeafNode(NodeData nodeData, LeafNode leafNode, RunnerCodeTemplate template)
        // {
        //     template.StartNodeCreation(nodeData, "CreateLeaf");
        //     template.AddActionParameter(leafNode.ActionReference, $"{nodeData.name.ToValidIdentificatorName()}_action");
        //     template.CloseNodeCreation();
        // }

        // private void GenerateCodeForCompositeNode(NodeData nodeData, BehaviourTrees.CompositeNode compositeNode, RunnerCodeTemplate template)
        // {
        //     template.StartNodeCreation(nodeData, "CreateDecorator", isGeneric: true);
        //     template.AddPrimitiveParameter(compositeNode.IsRandomized);
        //     for (int i = 0; i < nodeData.childIds.Count; i++)
        //     {
        //         NodeData data = FindNodeById(nodeData.childIds[i]);
        //         if (data != null && !Exists(data))
        //         {
        //             GenerateBTNodeCode(data);
        //         }
        //         template.AddNodeParameter(data);
        //     }
        //     template.CloseNodeCreation();
        // }

        // private void GenerateCodeForDecoratorNode(NodeData nodeData, BehaviourTrees.DecoratorNode decoratorNode, RunnerCodeTemplate template)
        // {

        // }

        public RunnerCodeTemplate()
        {

        }

        public string GenerateCode(string className, SystemData data)
        {
            if (data == null || data.graphs.Count == 0) return null;

            CodeCompileUnit unit = new CodeCompileUnit();

            m_namespace = new CodeNamespace();

            m_namespace.Imports.Add(new CodeNamespaceImport("BehaviourAPI.Unity.Runtime"));

            unit.Namespaces.Add(m_namespace);

            m_classDeclaration = new CodeTypeDeclaration(className);
            m_classDeclaration.BaseTypes.Add(new CodeTypeReference(nameof(CodeBehaviourRunner)));
            m_namespace.Types.Add(m_classDeclaration);

            m_mainMemberMethod = AddCreateGraphMethod();
            m_classDeclaration.Members.Add(m_mainMemberMethod);

            //GenerateBaseCode(className);

            foreach (GraphData graphData in data.graphs)
            {
                GenerateGraphDeclaration(graphData);
            }

            foreach (GraphData graphData in data.graphs)
            {
                GenerateNodeDeclarations(graphData);
            }

            foreach (PushPerceptionData pushPerceptionData in data.pushPerceptions)
            {

            }

            StringBuilder generatedCode = new StringBuilder();
            StringWriter codeWriter = new StringWriter(generatedCode);
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            codeProvider.GenerateCodeFromCompileUnit(unit, codeWriter, options);

            return generatedCode.ToString();
        }

        private void GenerateGraphDeclaration(GraphData graphData)
        {
            if (graphData == null || graphData.graph == null) return;

            Type graphType = graphData.graph.GetType();
            m_namespace.Imports.Add(new CodeNamespaceImport(graphType.Namespace));
            CodeTypeReference graphTypeReference = new CodeTypeReference(graphType.Name);
            string graphName = GetValidIdentificator(graphData.name, m_mainMemberMethod);
            CodeVariableDeclarationStatement graphDeclaration = new CodeVariableDeclarationStatement(graphTypeReference, graphName);
            CodeObjectCreateExpression graphCreateExpression = new CodeObjectCreateExpression(graphTypeReference);
            graphDeclaration.InitExpression = graphCreateExpression;
            m_mainMemberMethod.Statements.Add(graphDeclaration);

            IEnumerable<FieldInfo> graphFields = graphType.GetFields();
            foreach (FieldInfo field in graphFields)
            {
                CodeFieldReferenceExpression graphFieldReferenceExpression = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(graphName), field.Name);
                CodeExpression graphFieldValueExpression = GetCodeExpression(field.GetValue(graphData.graph), $"{graphName}_{field.Name}");
                CodeAssignStatement graphFieldAssignStatement = new CodeAssignStatement(graphFieldReferenceExpression, graphFieldValueExpression);
                m_mainMemberMethod.Statements.Add(graphFieldAssignStatement);
            }
        }

        private void GenerateNodeDeclarations(GraphData graphData)
        {
            if (graphData == null || graphData.graph == null) return;

            m_currentGraphReference = (CodeVariableReferenceExpression)m_mainMemberMethod.UserData[graphData];


        }

    }

}

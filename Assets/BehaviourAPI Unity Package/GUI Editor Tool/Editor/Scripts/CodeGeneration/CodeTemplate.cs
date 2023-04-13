using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Editor
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
    using BehaviourAPI.Unity.Framework.Adaptations;
    using BehaviourAPI.UnityExtensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class CodeTemplate
    {
        private static readonly string[] k_BaseNamespaces = new string[]
        {
            "System",
            "System.Collections.Generic",
            "UnityEngine",
            "BehaviourAPI.Core",
            "BehaviourAPI.Core.Actions",
            "BehaviourAPI.Core.Perceptions",
            "BehaviourAPI.UnityExtensions",
            "BehaviourAPI.Unity.Runtime"
        };

        IdentificatorProvider identificatorProvider = new IdentificatorProvider();

        Dictionary<string, string> m_SystemElementIdentificatorMap = new Dictionary<string, string>();

        Dictionary<Type, string> m_ComponentReferenceIdentificatorMap = new Dictionary<Type, string>();

        List<CodeFieldMember> m_FieldMembers = new List<CodeFieldMember>();

        List<CodeStatement> m_CodeGraphStatements = new List<CodeStatement>();
        List<CodeStatement> m_CodeStatements = new List<CodeStatement>();

        Dictionary<string, CodeMethodMember> m_MethodMembers = new Dictionary<string, CodeMethodMember>();

        HashSet<string> m_UsingNamespaces = new HashSet<string>();

        private SystemData m_SystemData;

        public void Create(SystemData systemData)
        {
            if (systemData == null) return;

            RegisterSystemElementIdentificators(systemData);

            foreach (string ns in k_BaseNamespaces)
            {
                m_UsingNamespaces.Add(ns);
            }

            foreach (GraphData graphData in systemData.graphs)
            {
                GraphCodeGenerator codeGenerator = GraphCodeGenerator.GetGenerator(graphData);
                codeGenerator.GenerateCode(this);
                m_CodeStatements.Add(new CustomStatement(""));
            }

            foreach (PushPerceptionData pushPerceptionData in systemData.pushPerceptions)
            {

            }

            if (systemData.graphs.Count > 0)
            {
                string rootGraphIdentificator = GetSystemElementIdentificator(systemData.graphs[0].id);
                m_CodeStatements.Add(new CustomStatement($"return {rootGraphIdentificator};"));
            }
            else
            {
                m_CodeStatements.Add(new CustomStatement($"return null;"));
            }

            m_SystemData = systemData;
        }

        private void RegisterSystemElementIdentificators(SystemData systemData)
        {
            foreach (GraphData graphData in systemData.graphs)
            {
                RegisterSystemElementIdentificator(graphData.id, graphData.name);

                foreach (NodeData nodeData in graphData.nodes)
                {
                    RegisterSystemElementIdentificator(nodeData.id, nodeData.name);
                }
            }
        }

        public void AddStatement(CodeStatement statement, bool isGraph = false)
        {
            if (isGraph) m_CodeGraphStatements.Add(statement);
            else m_CodeStatements.Add(statement);
        }

        //public void GenerateVariableAssignations(object obj, string identifier)
        //{
        //    var type = obj.GetType();
        //    var fields = type.GetFields();

        //    foreach (var field in fields)
        //    {
        //        var statement = new CodeVariableReassignationStatement();
        //        statement.LeftExpression = new CodeMethodReferenceExpression(identifier, field.Name);

        //        if (typeof(Action).IsAssignableFrom(field.FieldType))
        //        {
        //            statement.RightExpression = GetActionExpression((Action)field.GetValue(obj));
        //        }
        //        else if (typeof(Perception).IsAssignableFrom(field.FieldType))
        //        {
        //            statement.RightExpression = GetPerceptionExpression((Perception)field.GetValue(obj));
        //        }
        //        else
        //        {
        //            statement.RightExpression = CreateGenericExpression("null");
        //        }
        //        m_CodeStatements.Add(statement);
        //    }
        //}

        public void AddNamespace(string ns)
        {
            m_UsingNamespaces.Add(ns);
        }

        // ===================================================================================================== //

        public CodeArgumentExpression CreateReferencedElementExpression(string id)
        {
            var identificator = m_SystemElementIdentificatorMap.GetValueOrDefault(id);
            return new CodeSimpleExpression(identificator);
        }

        public CodeArgumentExpression CreateGenericExpression(string code)
        {
            return new CodeSimpleExpression(code);
        }

        public CodeArgumentExpression GetActionExpression(Core.Actions.Action action)
        {
            if (action != null)
            {
                CodeObjectCreationExpression expression = null;

                switch (action)
                {
                    case Framework.Adaptations.CustomAction custom:
                        expression = new CodeObjectCreationExpression(typeof(FunctionalAction));
                        CodeArgumentExpression startMethodArg = GenerateMethodCodeExpression(custom.start, null);
                        CodeArgumentExpression updateMethodArg = GenerateMethodCodeExpression(custom.update, null, typeof(Core.Status));
                        CodeArgumentExpression stopMethodArg = GenerateMethodCodeExpression(custom.stop, null);

                        if (startMethodArg != null) expression.Add(startMethodArg);
                        if (updateMethodArg != null) expression.Add(updateMethodArg);
                        else expression.Add(new CodeSimpleExpression("() => Status.Running"));
                        if (stopMethodArg != null) expression.Add(stopMethodArg);
                        break;
                    case UnityAction unityAction:
                        break;

                    case SubgraphAction subgraphAction:
                        expression = new CodeObjectCreationExpression(typeof(SubsystemAction));

                        expression.Add(CreateReferencedElementExpression(subgraphAction.subgraphId));
                        expression.Add(new CodeSimpleExpression(subgraphAction.DontStopOnInterrupt.ToCodeFormat()));
                        expression.Add(new CodeSimpleExpression(subgraphAction.ExecuteOnLoop.ToCodeFormat()));
                        break;
                }
                return expression;
            }
            else
            {
                return new CodeSimpleExpression("null /*missing action*/");
            }
        }

        public CodeArgumentExpression GetPerceptionExpression(Perception perception)
        {
            if (perception != null)
            {
                CodeObjectCreationExpression expression = null;

                switch (perception)
                {
                    case Framework.Adaptations.CustomPerception custom:
                        expression = new CodeObjectCreationExpression(typeof(ConditionPerception));
                        CodeArgumentExpression startMethodArg = GenerateMethodCodeExpression(custom.init, null);
                        CodeArgumentExpression updateMethodArg = GenerateMethodCodeExpression(custom.check, null, typeof(bool));
                        CodeArgumentExpression stopMethodArg = GenerateMethodCodeExpression(custom.reset, null);

                        if (startMethodArg != null) expression.Add(startMethodArg);
                        if (updateMethodArg != null) expression.Add(updateMethodArg);
                        else expression.Add(new CodeSimpleExpression("() => Status.Running"));
                        if (stopMethodArg != null) expression.Add(stopMethodArg);
                        break;
                    case UnityPerception unityPerception:
                        break;

                    case CompoundPerceptionWrapper compoundPerception:
                        expression = new CodeObjectCreationExpression(compoundPerception.compoundPerception.GetType());

                        foreach (var subperception in compoundPerception.subPerceptions)
                        {
                            expression.Add(GetPerceptionExpression(subperception.perception));
                        }
                        break;
                }

                if (expression != null) return expression;
            }
            return new CodeSimpleExpression("null /*missing action*/");

        }

        public CodeArgumentExpression GenerateMethodCodeExpression(SerializedContextMethod serializedMethod, Type[] args, Type returnType = null)
        {
            if (string.IsNullOrWhiteSpace(serializedMethod.methodName))
            {
                return null;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(serializedMethod.componentName))
                {
                    if (BehaviourAPISettings.instance.Metadata.componentMap.TryGetValue(serializedMethod.componentName, out Type componentType) &&
                        CheckIfMethodExists(componentType, serializedMethod.methodName, args))
                    {
                        string componentIdentificator = GetOrCreateLocalComponentReference(componentType);
                        return new CodeMethodReferenceExpression(componentIdentificator, serializedMethod.methodName);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    string methodIdentificator = GetOrCreateLocalMethod(serializedMethod.methodName, args, returnType);
                    return new CodeMethodReferenceExpression(methodIdentificator);
                }
            }
        }

        // ===================================================================================================== //

        private bool CheckIfMethodExists(Type type, string methodName, Type[] argumentTypes)
        {
            var methodInfo = type.GetMethod("methodName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null,
                System.Reflection.CallingConventions.Any, argumentTypes, null);
            return methodInfo != null;
        }

        private string GetOrCreateLocalMethod(string methodName, Type[] args, Type returnType)
        {
            if (!m_MethodMembers.TryGetValue(methodName, out var member))
            {
                var localMethod = new CodeMethodMember()
                {
                    Name = methodName,
                    returnType = returnType,
                    parameterExpressions = args?.Select(arg => new CodeParameterExpression()
                    {
                        type = arg,
                        name = "arg"
                    }).ToList()
                };
                m_MethodMembers[methodName] = localMethod;
            }
            return methodName;
        }

        private string GetOrCreateLocalComponentReference(Type componentType)
        {
            return componentType.Name;
        }

        // Registra el identificador de un nodo o grafo
        private void RegisterSystemElementIdentificator(string id, string name)
        {
            string identificator = identificatorProvider.GenerateIdentificator(name);
            m_SystemElementIdentificatorMap[id] = identificator;
        }

        // ===================================================================================================== //

        public string GenerateCode(string value, CodeGenerationOptions options)
        {
            using (new DebugTimer())
            {
                CodeWritter codeWritter = new CodeWritter();

                foreach (var usingNamespace in m_UsingNamespaces)
                {
                    codeWritter.AppendLine($"using {usingNamespace};");
                }

                codeWritter.AppendLine("");

                if (!string.IsNullOrWhiteSpace(options.scriptNamespace))
                {
                    codeWritter.AppendLine("namespace " + options.scriptNamespace);
                    codeWritter.AppendLine("{");
                    codeWritter.IdentationLevel++;
                }

                codeWritter.AppendLine($"public class {value} : CodeBehaviourRunner");
                codeWritter.AppendLine("{");

                codeWritter.IdentationLevel++;

                foreach (var fieldCode in m_FieldMembers)
                {
                    fieldCode.GenerateCode(codeWritter, options);
                }

                codeWritter.AppendLine("");

                codeWritter.AppendLine("protected override BehaviourGraph CreateGraph()");
                codeWritter.AppendLine("{");

                codeWritter.IdentationLevel++;

                m_CodeGraphStatements.ForEach(c => c.GenerateCode(codeWritter, options));
                codeWritter.AppendLine("");
                m_CodeStatements.ForEach(c => c.GenerateCode(codeWritter, options));

                codeWritter.IdentationLevel--;

                codeWritter.AppendLine("}");

                foreach (var method in m_MethodMembers.Values)
                {
                    method.GenerateCode(codeWritter, options);
                }

                codeWritter.IdentationLevel--;

                codeWritter.AppendLine("}");

                if (!string.IsNullOrWhiteSpace(options.scriptNamespace))
                {
                    codeWritter.IdentationLevel--;
                    codeWritter.AppendLine("}");
                }

                return codeWritter.ToString();
            }
        }

        public string GetSystemElementIdentificator(string elementId)
        {
            return m_SystemElementIdentificatorMap.GetValueOrDefault(elementId);
        }

        public CodeArgumentExpression CreatePropertyExpression(object obj)
        {
            return new CodeSimpleExpression(ToCode(obj));
        }

        private string ToCode(object obj)
        {
            var type = obj.GetType();
            if (type.IsEnum)
            {
                return $"{type.Name}.{obj}";
            }
            else if (type.IsValueType)
            {
                if (obj is int i) return i.ToString();
                else if (obj is float f) return f.ToCodeFormat();
                else if (obj is bool b) return b.ToCodeFormat();
                else if (obj is char c) return $"\'{c}\'";
                else if (obj is Vector2 v2) return $"new Vector2({ToCode(v2.x)}, {ToCode(v2.y)})";
                else if (obj is Vector3 v3) return $"new Vector3({ToCode(v3.x)}, {ToCode(v3.y)}, {ToCode(v3.z)})";
                else return "null";
            }
            else
            {
                var identificator = identificatorProvider.GenerateIdentificator("_" + type.Name.ToLower());
                m_FieldMembers.Add(new CodeFieldMember()
                {
                    type = type,
                    name = identificator
                });
                return identificator;
            }
        }
    }
}

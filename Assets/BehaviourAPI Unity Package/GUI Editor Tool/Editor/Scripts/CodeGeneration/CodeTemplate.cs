using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    using BehaviourAPI.Core.Actions;
    using BehaviourAPI.Core.Perceptions;
    using BehaviourAPI.Unity.Framework.Adaptations;
    using BehaviourAPI.UnityExtensions;
    using BehaviourAPI.UtilitySystems;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Action = Core.Actions.Action;

    public class CodeTemplate
    {
        #region ------------------------------- Private fields -------------------------------

        private static readonly string[] k_BaseNamespaces = new string[]
        {
            "System.Collections.Generic",
            "UnityEngine",
            "BehaviourAPI.Core",
            "BehaviourAPI.Core.Actions",
            "BehaviourAPI.Core.Perceptions",
            "BehaviourAPI.UnityExtensions",
            "BehaviourAPI.Unity.Runtime"
        };

        SystemData m_SystemData;

        IdentificatorProvider identificatorProvider = new IdentificatorProvider();

        Dictionary<string, string> m_SystemElementIdentificatorMap = new Dictionary<string, string>();

        Dictionary<Type, string> m_ComponentReferenceIdentificatorMap = new Dictionary<Type, string>();

        List<CodeFieldMember> m_FieldMembers = new List<CodeFieldMember>();

        List<CodeStatement> m_CodeGraphStatements = new List<CodeStatement>();
        List<CodeStatement> m_CodePropertiesStatements = new List<CodeStatement>();
        List<CodeStatement> m_CodeStatements = new List<CodeStatement>();

        Dictionary<string, CodeMethodMember> m_MethodMembers = new Dictionary<string, CodeMethodMember>();

        HashSet<string> m_UsingNamespaces = new HashSet<string>();

        #endregion

        /// <summary>
        /// Create a new CodeTemplate with a <see cref="SystemData"/>.
        /// </summary>
        /// <param name="systemData">The data used to generate the code.</param>
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
                m_CodeStatements.Add(new CodeCustomStatement(""));
            }

            foreach (PushPerceptionData pushPerceptionData in systemData.pushPerceptions)
            {

            }

            m_SystemData = systemData;
        }

        /// <summary>
        /// Add a new graph creation line in the main method.
        /// </summary>
        /// <param name="statement">The statement added.</param>
        public void AddGraphCreationStatement(CodeStatement statement)
        {
            m_CodeGraphStatements.Add(statement);
        }

        /// <summary>
        /// Add a new node creation line in the graph creation method.
        /// </summary>
        /// <param name="statement">The statement added</param>
        public void AddStatement(CodeStatement statement)
        {
            m_CodeStatements.Add(statement);
            foreach (var st in m_CodePropertiesStatements)
            {
                m_CodeStatements.Add(st);
            }
            m_CodePropertiesStatements.Clear();
        }

        /// <summary>
        /// Add a new statement that will be added in code after the next statement.
        /// </summary>
        /// <param name="statement">The statement added.</param>
        public void AddPropertyStatement(CodeStatement statement)
        {
            m_CodePropertiesStatements.Add(statement);
        }

        /// <summary>
        /// Add a new namespace to the template if wasn't added yet.
        /// </summary>
        /// <param name="ns">The new namespace added.</param>
        public void AddNamespace(string ns)
        {
            m_UsingNamespaces.Add(ns);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="identificator"></param>
        /// <param name="inline"></param>
        /// <returns></returns>
        public CodeExpression GetActionExpression(Action action, string identificator)
        {
            if (action != null)
            {
                var id = identificatorProvider.GenerateIdentificator(identificator);
                CodeVariableDeclarationStatement statement;
                switch (action)
                {
                    case CustomAction custom:
                        var expression = new CodeObjectCreationExpression(typeof(FunctionalAction));
                        CodeExpression startMethodArg = GenerateMethodCodeExpression(custom.start, null);
                        CodeExpression updateMethodArg = GenerateMethodCodeExpression(custom.update, null, typeof(Core.Status));
                        CodeExpression stopMethodArg = GenerateMethodCodeExpression(custom.stop, null);
                        if (startMethodArg != null) expression.Add(startMethodArg);
                        if (updateMethodArg != null) expression.Add(updateMethodArg);
                        else expression.Add(new CodeCustomExpression("() => Status.Running"));
                        if (stopMethodArg != null) expression.Add(stopMethodArg);
                        statement = new CodeVariableDeclarationStatement(typeof(FunctionalAction), id);
                        statement.RightExpression = expression;
                        break;

                    case UnityAction unityAction:
                        var type = unityAction.GetType();
                        expression = new CodeObjectCreationExpression(type);
                        var fields = type.GetFields();
                        foreach (var field in fields) AddPropertyStatement(field.GetValue(action), id, field.Name);
                        statement = new CodeVariableDeclarationStatement(type, id);
                        statement.RightExpression = expression;
                        break;

                    case SubgraphAction subgraphAction:
                        expression = new CodeObjectCreationExpression(typeof(SubsystemAction));
                        var subgraphId = GetSystemElementIdentificator(subgraphAction.subgraphId);

                        if (subgraphId != null)
                        {
                            expression.Add(new CodeCustomExpression(subgraphId));
                        }
                        else
                        {
                            expression.Add(new CodeCustomExpression("null /*missing subgraph*/"));
                        }

                        if (subgraphAction.ExecuteOnLoop || subgraphAction.DontStopOnInterrupt)
                        {
                            expression.Add(new CodeCustomExpression(subgraphAction.DontStopOnInterrupt.ToCodeFormat()));
                            expression.Add(new CodeCustomExpression(subgraphAction.ExecuteOnLoop.ToCodeFormat()));
                        }
                        statement = new CodeVariableDeclarationStatement(typeof(SubsystemAction), id);
                        statement.RightExpression = expression;
                        break;
                    default:
                        statement = null;
                        break;

                }
                if (statement != null)
                {
                    AddStatement(statement);
                    return new CodeCustomExpression(id);
                }
                else
                {
                    return new CodeCustomExpression("null /*missing perception*/");
                }
            }
            return new CodeCustomExpression("null /*missing action*/");

        }

        /// <summary>
        /// Generates a code expression for a perception
        /// </summary>
        /// <param name="perception"></param>
        /// <param name="identificator"></param>
        /// <param name="inline"></param>
        /// <returns></returns>
        public CodeExpression GetPerceptionExpression(Perception perception, string identificator)
        {
            if (perception != null)
            {
                var id = identificatorProvider.GenerateIdentificator(identificator);
                CodeVariableDeclarationStatement statement;
                switch (perception)
                {
                    case CustomPerception custom:
                        var expression = new CodeObjectCreationExpression(typeof(ConditionPerception));
                        CodeExpression startMethodArg = GenerateMethodCodeExpression(custom.init, null);
                        CodeExpression updateMethodArg = GenerateMethodCodeExpression(custom.check, null, typeof(bool));
                        CodeExpression stopMethodArg = GenerateMethodCodeExpression(custom.reset, null);
                        if (startMethodArg != null) expression.Add(startMethodArg);
                        if (updateMethodArg != null) expression.Add(updateMethodArg);
                        else expression.Add(new CodeCustomExpression("() => Status.Running"));
                        if (stopMethodArg != null) expression.Add(stopMethodArg);
                        statement = new CodeVariableDeclarationStatement(typeof(ConditionPerception), id);
                        statement.RightExpression = expression;
                        break;

                    case UnityPerception unityPerception:
                        var type = unityPerception.GetType();
                        expression = new CodeObjectCreationExpression(type);
                        var fields = type.GetFields();
                        foreach (var field in fields) AddPropertyStatement(field.GetValue(perception), id, field.Name);
                        statement = new CodeVariableDeclarationStatement(type, id);
                        statement.RightExpression = expression;
                        break;

                    case CompoundPerceptionWrapper compoundPerception:
                        expression = new CodeObjectCreationExpression(compoundPerception.compoundPerception.GetType());
                        for (int i = 0; i < compoundPerception.subPerceptions.Count; i++)
                        {
                            var subperception = compoundPerception.subPerceptions[i];
                            expression.Add(GetPerceptionExpression(subperception.perception, identificator + "_sub" + (i + 1)));
                        }
                        statement = new CodeVariableDeclarationStatement(compoundPerception.compoundPerception.GetType(), id);
                        statement.RightExpression = expression;
                        break;

                    default:
                        statement = null;
                        break;
                }

                if (statement != null)
                {
                    AddStatement(statement);
                    return new CodeCustomExpression(id);
                }
                else
                {
                    return new CodeCustomExpression("null /*missing perception*/");
                }
            }
            return new CodeCustomExpression("null /*missing perception*/");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializedMethod"></param>
        /// <param name="args"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public CodeExpression GenerateMethodCodeExpression(SerializedContextMethod serializedMethod, Type[] args, Type returnType = null)
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
                var localMethod = new CodeMethodMember(methodName, returnType);

                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        localMethod.Parameters.Add(new CodeParameter(args[i], args[i].Name + "_" + i));
                    }
                }
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


        /// <summary>
        /// Generate the code 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public string GenerateCode(string value, CodeGenerationOptions options)
        {
            using (new DebugTimer())
            {
                CodeWriter codeWritter = new CodeWriter();

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

                var firstGraphId = m_SystemData.graphs.FirstOrDefault()?.id;
                if (options.registerGraphsInDebugger)
                {
                    foreach (var graph in m_SystemData.graphs)
                    {
                        codeWritter.AppendLine($"RegisterGraph({m_SystemElementIdentificatorMap[graph.id]});");
                    }
                    codeWritter.AppendLine("");
                }

                if (firstGraphId != null) codeWritter.AppendLine($"return {m_SystemElementIdentificatorMap[firstGraphId]};");
                else codeWritter.Append("return null");


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

        public void AddPropertyStatement(object obj, string identifier, string name)
        {
            var statement = new CodeAssignationStatement();
            statement.LeftExpression = new CodeMethodReferenceExpression(identifier, name);

            switch (obj)
            {
                case Action action:
                    statement.RightExpression = GetActionExpression(action, identifier); break;
                case Perception perception:
                    statement.RightExpression = GetPerceptionExpression(perception, identifier); break;
                default:
                    statement.RightExpression = CreateGenericExpression(obj, identifier + "_" + name); break;
            }

            AddPropertyStatement(statement);
        }

        private CodeExpression CreateGenericExpression(object obj, string defaultIdentificatorName)
        {
            var type = obj.GetType();

            if (type.IsEnum)
            {
                return new CodeCustomExpression($"{type.Name}.{obj}");
            }
            else if (type.IsArray)
            {
                switch (obj)
                {
                    case int[] i:
                        return new CodeCustomExpression($"new int[] {{ {i.Select(i => i.ToString()).Join()}}}");
                    case float[] f:
                        return new CodeCustomExpression($"new float[] {{ {f.Select(f => f.ToCodeFormat()).Join()}}}");
                    case bool[] b:
                        return new CodeCustomExpression($"new bool[] {{ {b.Select(b => b.ToCodeFormat()).Join()}}}");
                    case char[] c:
                        return new CodeCustomExpression($"new bool[] {{ {c.Select(c => $"\'{c}\'").Join()}}}");
                }
            }
            else if (type.IsValueType)
            {
                switch (obj)
                {
                    case int i:
                        return new CodeCustomExpression(i.ToString());
                    case float f:
                        return new CodeCustomExpression(f.ToCodeFormat());
                    case bool b:
                        return new CodeCustomExpression(b.ToCodeFormat());
                    case char c:
                        return new CodeCustomExpression($"\'{c}\'");
                    case Vector2 v2:
                        var expression = new CodeObjectCreationExpression(typeof(Vector2));
                        expression.Add(new CodeCustomExpression(v2.x.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(v2.y.ToCodeFormat()));
                        return expression;
                    case Vector3 v3:
                        expression = new CodeObjectCreationExpression(typeof(Vector3));
                        expression.Add(new CodeCustomExpression(v3.x.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(v3.y.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(v3.z.ToCodeFormat()));
                        return expression;
                    case Color color:
                        expression = new CodeObjectCreationExpression(typeof(Vector3));
                        expression.Add(new CodeCustomExpression(color.r.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(color.g.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(color.b.ToCodeFormat()));
                        expression.Add(new CodeCustomExpression(color.a.ToCodeFormat()));
                        return expression;
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                switch (obj)
                {
                    case List<int> i:
                        return new CodeCustomExpression($"new List<int>() {{{i.Select(i => i.ToString()).Join()}}}");
                    case List<float> f:
                        return new CodeCustomExpression($"new List<float>() {{{f.Select(i => i.ToCodeFormat()).Join()}}}");
                    case List<bool> b:
                        return new CodeCustomExpression($"new List<bool>() {{{b.Select(i => i.ToCodeFormat()).Join()}}}");
                    case List<char> c:
                        return new CodeCustomExpression($"new List<char>() {{{c.Select(i => $"\'i\'").Join()}}}");
                    case List<CurvePoint> c:
                        return new CodeCustomExpression($"new List<CurvePoint>() {{{c.Select(i => $"new CurvePoint({i.x.ToCodeFormat()}, {i.y.ToCodeFormat()})").Join()}}}");
                }
            }

            var identificator = identificatorProvider.GenerateIdentificator(defaultIdentificatorName);
            m_FieldMembers.Add(new CodeFieldMember(identificator, type));
            return new CodeCustomExpression(identificator);
        }

    }
}

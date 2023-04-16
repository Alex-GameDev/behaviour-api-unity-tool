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
    using System.Text.RegularExpressions;
    using UnityEngine;
    using Action = Core.Actions.Action;

    public class CodeTemplate
    {
        #region ------------------------------- Private fields -------------------------------

        private static readonly string[] k_Keywords = new[]
{
            "bool", "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "double", "float", "decimal",
            "string", "char", "void", "object", "typeof", "sizeof", "null", "true", "false", "if", "else", "while", "for", "foreach", "do", "switch",
            "case", "default", "lock", "try", "throw", "catch", "finally", "goto", "break", "continue", "return", "public", "private", "internal",
            "protected", "static", "readonly", "sealed", "const", "fixed", "stackalloc", "volatile", "new", "override", "abstract", "virtual",
            "event", "extern", "ref", "out", "in", "is", "as", "params", "__arglist", "__makeref", "__reftype", "__refvalue", "this", "base",
            "namespace", "using", "class", "struct", "interface", "enum", "delegate", "checked", "unchecked", "unsafe", "operator", "implicit", "explicit"
        };


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

        Dictionary<string, string> m_SystemElementIdentifierMap = new Dictionary<string, string>();

        Dictionary<Type, string> m_ComponentReferenceIdentifierMap = new Dictionary<Type, string>();

        List<CodeFieldMember> m_FieldMembers = new List<CodeFieldMember>();

        List<CodeStatement> m_CodeGraphStatements = new List<CodeStatement>();
        List<CodeStatement> m_CodePropertiesStatements = new List<CodeStatement>();
        List<CodeStatement> m_CodeStatements = new List<CodeStatement>();

        Dictionary<string, CodeMethodMember> m_MethodMembers = new Dictionary<string, CodeMethodMember>();

        HashSet<string> m_UsingNamespaces = new HashSet<string>();

        HashSet<string> m_UsedIdentifiers = new HashSet<string>();


        #endregion

        /// <summary>
        /// Create a new CodeTemplate with a <see cref="SystemData"/>.
        /// </summary>
        /// <param name="systemData">The data used to generate the code.</param>
        public void Create(SystemData systemData)
        {
            if (systemData == null) return;

            RegisterSystemElementIdentifiers(systemData);

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
                var id = GenerateIdentifier(pushPerceptionData.name);
                var field = new CodeFieldMember(id, typeof(PushPerception), isSerializeField: false);
                m_FieldMembers.Add(field);
                var pushStatement = new CodeAssignationStatement();
                pushStatement.LeftExpression = new CodeMethodReferenceExpression(id);
                var createExpression = new CodeObjectCreationExpression(typeof(PushPerception));
                pushStatement.RightExpression = createExpression;
                foreach (var target in pushPerceptionData.targetNodeIds)
                {
                    var targetId = GetSystemElementIdentifier(target);
                    createExpression.Add(new CodeCustomExpression(targetId));
                }
                m_CodeStatements.Add(pushStatement);
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
        /// Add a new namespace to the template if wasn't added yet.
        /// </summary>
        /// <param name="ns">The new namespace added.</param>
        public void AddNamespace(string ns)
        {
            m_UsingNamespaces.Add(ns);
        }

        /// <summary>
        /// Generates a code expression for an action.
        /// </summary>
        /// <param name="action">The action converted to code.</param>
        /// <param name="Identifier">The base Identifier for the action.</param>
        /// <returns>A code expression with the identifier of the action created</returns>
        public CodeExpression GetActionExpression(Action action, string Identifier)
        {
            if (action != null)
            {
                var id = GenerateIdentifier(Identifier);
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
                        var subgraphId = GetSystemElementIdentifier(subgraphAction.subgraphId);

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
        /// <param name="action">The perception converted to code.</param>
        /// <param name="Identifier">The base Identifier for the perception.</param>
        /// <returns>A code expression with the identifier of the perception created</returns>
        public CodeExpression GetPerceptionExpression(Perception perception, string Identifier)
        {
            if (perception != null)
            {
                var id = GenerateIdentifier(Identifier);
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
                            expression.Add(GetPerceptionExpression(subperception.perception, Identifier + "_sub" + (i + 1)));
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
        /// Create a code expression for a method. If the method is local, generate a method member in the template.
        /// </summary>
        /// <param name="serializedMethod">The data that contains the method and component name.</param>
        /// <param name="args">The type of arguments that the method must have.</param>
        /// <param name="returnType">The return type of the method. Void if null.</param>
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
                        string componentIdentifier = GetOrCreateLocalComponentReference(componentType);
                        return new CodeMethodReferenceExpression(componentIdentifier, serializedMethod.methodName);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    string methodIdentifier = GetOrCreateLocalMethod(serializedMethod.methodName, args, returnType);
                    return new CodeMethodReferenceExpression(methodIdentifier);
                }
            }
        }

        /// <summary>
        /// Return the identifier reserved for a graph or node by its id.
        /// </summary>
        /// <param name="elementId">The id of the node or graph.</param>
        /// <returns>The variable name used for the element.</returns>
        public string GetSystemElementIdentifier(string elementId)
        {
            return m_SystemElementIdentifierMap.GetValueOrDefault(elementId);
        }

        /// <summary>
        /// Add a new statement that will be included just after the next added to modify one of its properties.
        /// </summary>
        /// <param name="obj">The reference of the element assigned to the property</param>
        /// <param name="identifier">The node identifier</param>
        /// <param name="name">The property name</param>
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

            m_CodePropertiesStatements.Add(statement);
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
                CodeWriter codeWriter = new CodeWriter();

                foreach (var usingNamespace in m_UsingNamespaces)
                {
                    codeWriter.AppendLine($"using {usingNamespace};");
                }

                codeWriter.AppendLine("");

                if (!string.IsNullOrWhiteSpace(options.scriptNamespace))
                {
                    codeWriter.AppendLine("namespace " + options.scriptNamespace);
                    codeWriter.AppendLine("{");
                    codeWriter.IdentationLevel++;
                }

                codeWriter.AppendLine($"public class {value} : CodeBehaviourRunner");
                codeWriter.AppendLine("{");

                codeWriter.IdentationLevel++;

                foreach (var fieldCode in m_FieldMembers)
                {
                    fieldCode.GenerateCode(codeWriter, options);
                }

                codeWriter.AppendLine("");

                if (m_ComponentReferenceIdentifierMap.Count > 0)
                {
                    codeWriter.AppendLine("protected override void OnAwake()");
                    codeWriter.AppendLine("{");
                    codeWriter.IdentationLevel++;

                    foreach (var kvp in m_ComponentReferenceIdentifierMap)
                    {
                        codeWriter.AppendLine($"{kvp.Value} = GetComponent<{kvp.Key.Name}>();");
                    }
                    codeWriter.AppendLine("");
                    codeWriter.AppendLine("base.OnAwake();");
                    codeWriter.IdentationLevel--;
                    codeWriter.AppendLine("}");
                }

                codeWriter.AppendLine("");

                codeWriter.AppendLine("protected override BehaviourGraph CreateGraph()");
                codeWriter.AppendLine("{");

                codeWriter.IdentationLevel++;

                m_CodeGraphStatements.ForEach(c => c.GenerateCode(codeWriter, options));
                codeWriter.AppendLine("");
                m_CodeStatements.ForEach(c => c.GenerateCode(codeWriter, options));

                var firstGraphId = m_SystemData.graphs.FirstOrDefault()?.id;
                if (options.registerGraphsInDebugger)
                {
                    foreach (var graph in m_SystemData.graphs)
                    {
                        codeWriter.AppendLine($"RegisterGraph({m_SystemElementIdentifierMap[graph.id]});");
                    }
                    codeWriter.AppendLine("");
                }

                if (firstGraphId != null) codeWriter.AppendLine($"return {m_SystemElementIdentifierMap[firstGraphId]};");
                else codeWriter.Append("return null");


                codeWriter.IdentationLevel--;

                codeWriter.AppendLine("}");

                foreach (var method in m_MethodMembers.Values)
                {
                    method.GenerateCode(codeWriter, options);
                }

                codeWriter.IdentationLevel--;

                codeWriter.AppendLine("}");

                if (!string.IsNullOrWhiteSpace(options.scriptNamespace))
                {
                    codeWriter.IdentationLevel--;
                    codeWriter.AppendLine("}");
                }

                return codeWriter.ToString();
            }
        }

        private void RegisterSystemElementIdentifiers(SystemData systemData)
        {
            foreach (GraphData graphData in systemData.graphs)
            {
                RegisterSystemElementIdentifier(graphData.id, graphData.name);

                foreach (NodeData nodeData in graphData.nodes)
                {
                    RegisterSystemElementIdentifier(nodeData.id, nodeData.name);
                }
            }
        }

        private bool CheckIfMethodExists(Type type, string methodName, Type[] argumentTypes)
        {
            if (argumentTypes == null) argumentTypes = new Type[0];
            var methodInfo = type.GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null,
                System.Reflection.CallingConventions.Any, argumentTypes, null);

            Debug.Log($"Method:  {type.Name}.{methodName} {(methodInfo == null ? "dont exists" : "exists")}");

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
            var id = GenerateIdentifier($"m_{componentType.Name}");
            var member = new CodeFieldMember(id, componentType);
            m_FieldMembers.Add(member);
            m_ComponentReferenceIdentifierMap[componentType] = id;
            return id;
        }

        private void RegisterSystemElementIdentifier(string id, string name)
        {
            string Identifier = GenerateIdentifier(name);
            m_SystemElementIdentifierMap[id] = Identifier;
        }

        private CodeExpression CreateGenericExpression(object obj, string defaultIdentifierName)
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

            var Identifier = GenerateIdentifier(defaultIdentifierName);
            m_FieldMembers.Add(new CodeFieldMember(Identifier, type));
            return new CodeCustomExpression(Identifier);
        }

        private string GenerateIdentifier(string defaultName)
        {
            var idName = ConvertToValidIdentifier(defaultName);

            int i = 1;
            string fixedName = idName;
            while (m_UsedIdentifiers.Contains(fixedName))
            {
                fixedName = idName + "_" + i;
                i++;
            }
            m_UsedIdentifiers.Add(fixedName);
            return fixedName;
        }

        private string ConvertToValidIdentifier(string input)
        {
            string validString = Regex.Replace(input, @"[^\w]+", "_");

            if (string.IsNullOrEmpty(validString)) return "unnamed";
            if (char.IsDigit(input[0])) validString = "_" + validString;
            if (k_Keywords.Contains(validString)) validString = "@" + validString;

            return validString;
        }

    }
}

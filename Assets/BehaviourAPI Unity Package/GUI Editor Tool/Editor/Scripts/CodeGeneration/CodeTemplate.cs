using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Editor.CodeGenerator
{
    using BehaviourAPI.Core;
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

    /// <summary>
    /// Class that represents the set of instructions needed to instantiate a node.
    /// </summary>
    public class CodeNodeStatementGroup
    {
        NodeData m_NodeData;

        CodeTemplate m_Template;

        List<CodeStatement> m_argumentStatements;
        CodeVariableDeclarationStatement m_NodeCreationStatement;
        CodeMethodInvokeExpression m_MethodInvoke;
        List<CodeStatement> m_PropertyStatements;


        public CodeNodeStatementGroup(NodeData nodeData, CodeTemplate template)
        {
            m_NodeData = nodeData;
            m_Template = template;
            string identifier = template.GetSystemElementIdentifier(nodeData.id);

            m_NodeCreationStatement = new CodeVariableDeclarationStatement(nodeData.node.GetType(), identifier);
            m_MethodInvoke = new CodeMethodInvokeExpression();
            m_NodeCreationStatement.RightExpression = m_MethodInvoke;

            m_argumentStatements = new List<CodeStatement>();
            m_PropertyStatements = new List<CodeStatement>();
        }

        public void Commit() => m_Template.AddNodeStatement(this);

        public void SetMethod(string k_method)
        {
            string graphIdentifier = m_Template.CurrentGraphIdentifier;
            m_MethodInvoke.methodReferenceExpression = new CodeMethodReferenceExpression(graphIdentifier, k_method);
        }

        public void AddFloat(float value)
        {
            m_MethodInvoke.parameters.Add(new CodeCustomExpression(value.ToCodeFormat()));
        }

        public void AddChildList()
        {
            for(int i = 0; i < m_NodeData.childIds.Count; i++)
            {
                string childIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.childIds[i]);
                var childExpression = new CodeCustomExpression(childIdentifier);
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddFirstChild()
        {
            if(m_NodeData.childIds.Count > 0)
            {
                string childIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.childIds[0]);
                var childExpression = new CodeCustomExpression(childIdentifier);
                m_MethodInvoke.parameters.Add(childExpression);
            }
            else
            {
                var childExpression = new CodeCustomExpression("null /* missing child */");
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddAction(string fieldName, bool isNotMandatory = false, string paramName = null)
        {
            var actionData = m_NodeData.references.Find((r) => r.FieldName == fieldName);

            if (actionData != null && actionData.Value is Action action)
            {
                var actionIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.id) + "_action";
                var actionExpression = m_Template.GetActionExpression(action, actionIdentifier, m_argumentStatements);
                m_MethodInvoke.parameters.Add(actionExpression);
            }
            else if(!isNotMandatory)
            {
                var childExpression = new CodeCustomExpression("null /* missing action */");
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddPerception(string fieldName, bool isNotMandatory = false, string paramName = null)
        {
            var perceptionData = m_NodeData.references.Find((r) => r.FieldName == fieldName);

            if (perceptionData != null && perceptionData.Value is Perception perception)
            {
                var perceptionIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.id) + "_perception";
                var perceptionExpression = m_Template.GetPerceptionExpression(perception, perceptionIdentifier, m_argumentStatements);
                m_MethodInvoke.parameters.Add(perceptionExpression);
            }
            else if (!isNotMandatory)
            {
                var childExpression = new CodeCustomExpression("null /* missing perception */");
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddStatusFlags(StatusFlags statusFlags, bool isNotMandatory = false, string paramName = null)
        {
            if ((int)statusFlags == -1 || statusFlags == StatusFlags.Active)
            {
                if (isNotMandatory) return;
                else statusFlags = StatusFlags.Active;
            }
            string paramPrefix = string.IsNullOrEmpty(paramName) ? "" : paramName + ": ";
            var statusExpression = new CodeCustomExpression(paramPrefix + "StatusFlags." + statusFlags);
            m_MethodInvoke.parameters.Add(statusExpression);
        }


        public void AddFirstParent(bool isNotMandatory = false)
        {
            if (m_NodeData.parentIds.Count > 0)
            {
                string parentIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.parentIds[0]);
                var parentExpression = new CodeCustomExpression(parentIdentifier);
                m_MethodInvoke.parameters.Add(parentExpression);
            }
            else if(!isNotMandatory)
            {
                var childExpression = new CodeCustomExpression("null /* missing parent */");
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddStatus(Status exitStatus)
        {
            var statusExpression = new CodeCustomExpression("Status." + exitStatus);
            m_MethodInvoke.parameters.Add(statusExpression);
        }

        public void AddFunction(string fieldName, bool isNotMandatory = false)
        {
            var methodData = m_NodeData.references.Find((r) => r.FieldName == fieldName);

            if (methodData != null && methodData.Value is SerializedContextMethod contextMethod)
            {
                var fieldInfo = m_NodeData.node.GetType().GetField(methodData.FieldName);

                if (fieldInfo == null || !fieldInfo.FieldType.IsSubclassOf(typeof(Delegate))) return;

                var methodInfo = fieldInfo.FieldType.GetMethod("Invoke");
                var returnParam = methodInfo.ReturnParameter.ParameterType;
                var args = methodInfo.GetParameters().Select(p => p.GetType()).ToArray();

                var functionIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.id) + "_function";
                var functionExpression = m_Template.GenerateMethodCodeExpression(contextMethod, args, returnParam);
                m_MethodInvoke.parameters.Add(functionExpression);
            }
            else if (!isNotMandatory)
            {
                var childExpression = new CodeCustomExpression("null /* missing action */");
                m_MethodInvoke.parameters.Add(childExpression);
            }
        }

        public void AddBool(bool b)
        {
            var boolExpression = new CodeCustomExpression(b.ToCodeFormat());
            m_MethodInvoke.parameters.Add(boolExpression);
        }

        public void AddPropertyAssignations()
        {
            string nodeIdentifier = m_Template.GetSystemElementIdentifier(m_NodeData.id);
            foreach(var field in m_NodeData.node.GetType().GetFields(System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance))
            {
                var value = field.GetValue(m_NodeData.node);
                if(value != null)
                {
                    var statement = m_Template.GetPropertyStatement(field.GetValue(m_NodeData.node), nodeIdentifier, field.Name, m_PropertyStatements);
                    m_PropertyStatements.Add(statement);
                }
            }  
        }

        public void GenerateCode(CodeWriter codeWriter, CodeGenerationOptions options)
        {
            m_argumentStatements.ForEach(s => s.GenerateCode(codeWriter, options));
            m_NodeCreationStatement.GenerateCode(codeWriter, options);
            m_PropertyStatements.ForEach(s => s.GenerateCode(codeWriter, options));
            codeWriter.AppendLine("");
        }
    }

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
            "System",
            "System.Collections.Generic",
            "UnityEngine",
            "BehaviourAPI.Core",
            "BehaviourAPI.Core.Actions",
            "BehaviourAPI.Core.Perceptions",
            "BehaviourAPI.UnityExtensions",
            "BehaviourAPI.Unity.Runtime"
        };

        SystemData m_SystemData;

        // Used to get the node and graph identifiers
        Dictionary<string, string> m_SystemElementIdentifierMap = new Dictionary<string, string>();

        // Used to get the identifiers of the local components
        Dictionary<Type, string> m_ComponentReferenceIdentifierMap = new Dictionary<Type, string>();

        // Used to get the local method names
        Dictionary<string, CodeMethodMember> m_MethodMembers = new Dictionary<string, CodeMethodMember>();

        // List of the imported namespaces
        HashSet<string> m_UsingNamespaces = new HashSet<string>();

        // Set of used variable names
        HashSet<string> m_UsedIdentifiers = new HashSet<string>();

        // Used to get the identifier used for an specified object
        Dictionary<object, string> m_FieldIdentifierMap = new Dictionary<object, string>();

        List<CodeFieldMember> m_FieldMembers = new List<CodeFieldMember>();
        List<CodeStatement> m_CodeGraphStatements = new List<CodeStatement>();
        List<CodeNodeStatementGroup> m_CodeNodeStatementList = new List<CodeNodeStatementGroup>();
        List<CodeStatement> m_CodeStatements = new List<CodeStatement>();

        public string CurrentGraphIdentifier { get; set; }

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
        public void AddNodeStatement(CodeNodeStatementGroup statement)
        {
            m_CodeNodeStatementList.Add(statement);
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
        /// <param name="identifier">The base Identifier for the action.</param>
        /// <returns>A code expression with the identifier of the action created</returns>
        public CodeExpression GetActionExpression(Action action, string identifier, List<CodeStatement> statements)
        {
            if (action == null) return new CodeCustomExpression("null /*missing action*/");

            identifier = GenerateIdentifier(identifier);
            switch (action)
            {
                case CustomAction custom:

                    var statement = new CodeVariableDeclarationStatement(typeof(FunctionalAction), identifier);
                    var expression = new CodeObjectCreationExpression(typeof(FunctionalAction));
                    statement.RightExpression = expression;
                    statements.Add(statement);

                    CodeExpression startMethodArg = GenerateMethodCodeExpression(custom.start, null);
                    CodeExpression updateMethodArg = GenerateMethodCodeExpression(custom.update, null, typeof(Status));
                    CodeExpression stopMethodArg = GenerateMethodCodeExpression(custom.stop, null);
                    CodeExpression pauseMethodArg = GenerateMethodCodeExpression(custom.pause, null);
                    CodeExpression unpauseMethodArg = GenerateMethodCodeExpression(custom.unpause, null);

                    if (startMethodArg != null)
                    {
                        var startStatement = new CodeAssignationStatement();
                        startStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onStarted");
                        startStatement.RightExpression = startMethodArg;
                        statements.Add(startStatement);
                    }

                    var updateStatement = new CodeAssignationStatement();
                    updateStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onUpdated");
                    if (updateMethodArg != null) updateStatement.RightExpression = updateMethodArg;
                    else updateStatement.RightExpression = new CodeCustomExpression("() => Status.Running");
                    statements.Add(updateStatement);

                    if (stopMethodArg != null)
                    {
                        var stopStatement = new CodeAssignationStatement();
                        stopStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onStopped");
                        stopStatement.RightExpression = stopMethodArg;
                        statements.Add(stopStatement);
                    }

                    if (pauseMethodArg != null)
                    {
                        var pauseStatement = new CodeAssignationStatement();
                        pauseStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onPause");
                        pauseStatement.RightExpression = pauseMethodArg;
                        statements.Add(pauseStatement);
                    }

                    if (unpauseMethodArg != null)
                    {
                        var unpauseStatement = new CodeAssignationStatement();
                        unpauseStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onUnpause");
                        unpauseStatement.RightExpression = unpauseMethodArg;
                        statements.Add(unpauseStatement);
                    }

                    return new CodeCustomExpression(identifier);

                case UnityAction unityAction:
                    var type = unityAction.GetType();
                    m_UsingNamespaces.Add(type.Namespace);
                    expression = new CodeObjectCreationExpression(type);

                    statement = new CodeVariableDeclarationStatement(type, identifier);
                    statement.RightExpression = expression;
                    statements.Add(statement);

                    var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
                    foreach (var field in fields)
                    {
                        var value = field.GetValue(unityAction);
                        if (value != null)
                        {
                            var st = GetPropertyStatement(field.GetValue(unityAction), identifier, field.Name, statements);
                            statements.Add(st);
                        }
                    }

                    return new CodeCustomExpression(identifier);

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
                    statement = new CodeVariableDeclarationStatement(typeof(SubsystemAction), identifier);
                    statement.RightExpression = expression;
                    statements.Add(statement);

                    return new CodeCustomExpression(identifier);
                default:
                    return new CodeCustomExpression("null /*missing perception*/");
            }
        }

        /// <summary>
        /// Generates a code expression for a perception
        /// </summary>
        /// <param name="action">The perception converted to code.</param>
        /// <param name="identifier">The base Identifier for the perception.</param>
        /// <returns>A code expression with the identifier of the perception created</returns>
        public CodeExpression GetPerceptionExpression(Perception perception, string identifier, List<CodeStatement> statements)
        {
            if (perception == null) new CodeCustomExpression("null /*missing perception*/");

            identifier = GenerateIdentifier(identifier);
            switch (perception)
            {
                case CustomPerception custom:
                    var statement = new CodeVariableDeclarationStatement(typeof(ConditionPerception), identifier);
                    var expression = new CodeObjectCreationExpression(typeof(ConditionPerception));
                    statement.RightExpression = expression;
                    statements.Add(statement);    
                        
                    CodeExpression startMethodArg = GenerateMethodCodeExpression(custom.init, null);
                    CodeExpression updateMethodArg = GenerateMethodCodeExpression(custom.check, null, typeof(bool));
                    CodeExpression stopMethodArg = GenerateMethodCodeExpression(custom.reset, null);
                    CodeExpression pauseMethodArg = GenerateMethodCodeExpression(custom.pause, null);
                    CodeExpression unpauseMethodArg = GenerateMethodCodeExpression(custom.unpause, null);

                    if (startMethodArg != null)
                    {
                        var startStatement = new CodeAssignationStatement();
                        startStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onInit");
                        startStatement.RightExpression = startMethodArg;
                        statements.Add(startStatement);
                    }

                    var updateStatement = new CodeAssignationStatement();
                    updateStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onCheck");
                    if (updateMethodArg != null) updateStatement.RightExpression = updateMethodArg;
                    else updateStatement.RightExpression = new CodeCustomExpression("() => false");
                    statements.Add(updateStatement);

                    if (stopMethodArg != null)
                    {
                        var stopStatement = new CodeAssignationStatement();
                        stopStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onReset");
                        stopStatement.RightExpression = stopMethodArg;
                        statements.Add(stopStatement);
                    }

                    if (pauseMethodArg != null)
                    {
                        var pauseStatement = new CodeAssignationStatement();
                        pauseStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onPause");
                        pauseStatement.RightExpression = pauseMethodArg;
                        statements.Add(pauseStatement);
                    }

                    if (unpauseMethodArg != null)
                    {
                        var unpauseStatement = new CodeAssignationStatement();
                        unpauseStatement.LeftExpression = new CodeMethodReferenceExpression(identifier, "onUnpause");
                        unpauseStatement.RightExpression = unpauseMethodArg;
                        statements.Add(unpauseStatement);
                    }

                    return new CodeCustomExpression(identifier);

                case UnityPerception unityPerception:
                    var type = unityPerception.GetType();
                    m_UsingNamespaces.Add(type.Namespace);
                    expression = new CodeObjectCreationExpression(type);

                    statement = new CodeVariableDeclarationStatement(type, identifier);
                    statement.RightExpression = expression;
                    statements.Add(statement);

                    var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy);
                    foreach (var field in fields)
                    {
                        var value = field.GetValue(unityPerception);
                        if (value != null)
                        {
                            var st = GetPropertyStatement(field.GetValue(perception), identifier, field.Name, statements);
                            statements.Add(st);
                        }
                    }

                    return new CodeCustomExpression(identifier);

                case CompoundPerceptionWrapper compoundPerception:
                    m_UsingNamespaces.Add(compoundPerception.GetType().Namespace);
                    expression = new CodeObjectCreationExpression(compoundPerception.compoundPerception.GetType());
                    for (int i = 0; i < compoundPerception.subPerceptions.Count; i++)
                    {
                        var subperception = compoundPerception.subPerceptions[i];
                        expression.Add(GetPerceptionExpression(subperception.perception, identifier + "_sub" + (i + 1), statements));
                    }
                    statement = new CodeVariableDeclarationStatement(compoundPerception.compoundPerception.GetType(), identifier);
                    statement.RightExpression = expression;
                    statements.Add(statement);

                    return new CodeCustomExpression(identifier);

                default:
                    break;

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
        public CodeStatement GetPropertyStatement(object obj, string identifier, string name, List<CodeStatement> statements)
        {
            if (obj == null) return null;
            var statement = new CodeAssignationStatement();
            statement.LeftExpression = new CodeMethodReferenceExpression(identifier, name);

            switch (obj)
            {
                case Action action:
                    statement.RightExpression = GetActionExpression(action, identifier, statements); break;
                case Perception perception:
                    statement.RightExpression = GetPerceptionExpression(perception, identifier, statements); break;
                default:
                    statement.RightExpression = CreateGenericExpression(obj, identifier + "_" + name); break;
            }
            return statement;
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
                m_CodeNodeStatementList.ForEach(c => c.GenerateCode(codeWriter, options));

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
                        expression = new CodeObjectCreationExpression(typeof(Color));
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

            if(m_FieldIdentifierMap.TryGetValue(obj, out string id))
            {
                return new CodeCustomExpression(id);
            }
            else
            {
                m_UsingNamespaces.Add(type.Namespace);
                var Identifier = GenerateIdentifier(defaultIdentifierName);
                m_FieldMembers.Add(new CodeFieldMember(Identifier, type));
                m_FieldIdentifierMap[obj] = Identifier;
                return new CodeCustomExpression(Identifier);
            }
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

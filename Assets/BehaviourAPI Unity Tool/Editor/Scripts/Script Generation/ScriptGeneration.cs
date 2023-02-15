using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;

using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.StateMachines;
using BehaviourAPI.StateMachines.StackFSMs;

using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using BehaviourAPI.UtilitySystems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

using Action = BehaviourAPI.Core.Actions.Action;

using Transition = BehaviourAPI.StateMachines.Transition;
using State = BehaviourAPI.StateMachines.State;
using ExitTransition = BehaviourAPI.StateMachines.ExitTransition;

using LeafNode = BehaviourAPI.BehaviourTrees.LeafNode;

using UtilityAction = BehaviourAPI.UtilitySystems.UtilityAction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;
using StateTransition = BehaviourAPI.StateMachines.StateTransition;
using PopTransition = BehaviourAPI.StateMachines.StackFSMs.PopTransition;
using PushTransition = BehaviourAPI.StateMachines.StackFSMs.PushTransition;
using UnityAction = BehaviourAPI.Unity.Runtime.Extensions.UnityAction;

namespace BehaviourAPI.Unity.Editor
{
    public static class ScriptGeneration
    {
        private static readonly string[] k_basicNamespaces =
        {
            "UnityEngine",
            "System.Collections",
            "System.Collections.Generic",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Core",
            "BehaviourAPI.Core.Actions",
            "BehaviourAPI.Core.Perceptions"
        };


        private static readonly string[] k_bannedNamespaces =
        {
            "BehaviourAPI.Unity.Framework.Adaptations"
        };

        private static readonly string k_CodeForMissingNode = "null /*Missing node*/";

        /// <summary>
        /// Create a script for a <see cref="BehaviourRunner"/> with all the data stored in <paramref name="asset"/>
        /// </summary>
        /// <param name="path">The destination path to the script.</param>
        /// <param name="name">The script class name.</param>
        /// <param name="asset">The system asset.</param>
        public static void GenerateScript(string path, string name, BehaviourSystemAsset asset, bool useFullNameVar = true, bool includeNodeNames = true)
        {
            if(asset == null)
            {
                Debug.LogWarning("Cant generate code of a null system");
                return;
            }

            var scriptPath = $"{path}{name}.cs";
            if(!string.IsNullOrEmpty(scriptPath))
            {
                Object obj = CreateScript(scriptPath, asset, useFullNameVar, includeNodeNames);
                AssetDatabase.Refresh();
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }

        static Object CreateScript(string path, BehaviourSystemAsset asset, bool useFullNameVar, bool includeNodeNames)
        {
            string folderPath = path.Substring(0, path.LastIndexOf("/") + 1);
            string scriptName = path.Substring(path.LastIndexOf("/") + 1).Replace(".cs", "");

            var content = GenerateSystemCode(asset, scriptName, useFullNameVar, includeNodeNames);

            UTF8Encoding encoding = new UTF8Encoding(true, false);
            StreamWriter writer = new StreamWriter(Path.GetFullPath(path), false, encoding);
            writer.Write(content);
            writer.Close();

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }

        /// <summary>
        /// Returns all the code to create the <paramref name="asset"/> in runtime.
        /// </summary>
        static string GenerateSystemCode(BehaviourSystemAsset asset, string scriptName, bool useFullNameVar, bool includeNames)
        {
            ScriptTemplate scriptTemplate = new ScriptTemplate(scriptName, useFullNameVar, nameof(CodeBehaviourRunner));

            foreach (var ns in k_basicNamespaces)
            {
                scriptTemplate.AddUsingDirective(ns);
            }

            scriptTemplate.OpenMethodDeclaration("CreateGraph", nameof(BehaviourGraph), 
                "protected override");


            //Graphs
            scriptTemplate.AddLine("/* --------------------------- GRAPHS: --------------------------- */");
            scriptTemplate.AddLine("");

            foreach (var graphAsset in asset.Graphs)
            {
                var graph = graphAsset.Graph;
                var graphName = graphAsset.Name;

                if (graph == null)
                {
                    Debug.LogWarning($"Graph {graphName} is empty.");
                    scriptTemplate.AddLine($"//Graph {graphName} is empty.");
                    continue;
                }

                if (string.IsNullOrEmpty(graphName)) graphName = graph.TypeName().ToLower();

                graphName = AddCreateGraphLine(graphAsset, graphName, scriptTemplate);
            }

            // Perceptions
            scriptTemplate.AddLine("");
            scriptTemplate.AddLine("/* --------------------------- Perceptions: --------------------------- */");
            scriptTemplate.AddLine("");

            foreach(var perceptionAsset in asset.Perceptions)
            {
                GenerateCodeForPullPerception(perceptionAsset, scriptTemplate);
            }

            // Nodes
            scriptTemplate.AddLine("");
            scriptTemplate.AddLine("/* --------------------------- Nodes: --------------------------- */");
            scriptTemplate.AddLine("");

            foreach (var graphAsset in asset.Graphs)
            {
                GenerateCodeForGraph(graphAsset, scriptTemplate, includeNames);
                scriptTemplate.AddLine("");
            }

            // Push perceptions
            scriptTemplate.AddLine("");
            scriptTemplate.AddLine("/* ------------------------- Push Perceptions: ------------------------- */");
            scriptTemplate.AddLine("");

            foreach(var pushPerception in asset.PushPerceptions)
            {
                GenerateCodeForPushPerception(pushPerception, scriptTemplate);
            }

            // Return
            if (asset.Graphs.Count > 0)
            {
                var mainGraphName = scriptTemplate.FindVariableName(asset.Graphs[0]);
                scriptTemplate.AddLine("return " + (mainGraphName ?? "null") + ";");
            }
            else
            {
                scriptTemplate.AddLine("return null;");
            }

            foreach (var ns in k_bannedNamespaces)
            {
                scriptTemplate.RemoveUsingDirective(ns);
            }

            scriptTemplate.CloseMethodDeclaration();

            return scriptTemplate.ToString();
        }

        /// <summary>
        /// Add a graph declaration line at the beginning of the method and return the graph variable name.
        /// </summary>
        static string AddCreateGraphLine(GraphAsset graphAsset, string graphName, ScriptTemplate template)
        {
            var graph = graphAsset.Graph;
            var type = graph.GetType();

            if (graph is UtilitySystem us)
            {
                graphName = template.AddVariableInstantiationLine(type, graphName, graphAsset, us.Inertia);
            }
            else
            {
                graphName = template.AddVariableInstantiationLine(type, graphName, graphAsset);
            }
            return graphName;
        }

        #region ---------------------------------- Actions and perceptions ----------------------------------
        /// <summary>
        /// Add the instruction line to create a push perception
        /// PushPerception p = new PushPerception(h1, h2, ...);
        /// </summary>
        /// <param name="pushPerception">The push perception asset</param>
        /// <param name="scriptTemplate">YThe script template</param>
        private static void GenerateCodeForPushPerception(PushPerceptionAsset pushPerception, ScriptTemplate scriptTemplate)
        {
            var targets = pushPerception.Targets.FindAll(tgt => tgt.Node is IPushActivable);
            scriptTemplate.AddVariableInstantiationLine(typeof(PushPerception), pushPerception.Name, pushPerception, targets);
        }

        private static string GenerateCodeForPullPerception(PerceptionAsset perception, ScriptTemplate scriptTemplate)
        {
            var p = perception.perception;
            var methodCode = "";

            scriptTemplate.AddUsingDirective(p.GetType().Namespace);
            var type = p.GetType();

            if (perception is CompoundPerceptionAsset cpa)
            {
                if(cpa.perception is CompoundPerception)
                {
                    var args = new List<string>();
                    foreach(var subPerception in cpa.subperceptions)
                    {
                        var subPerceptionName = scriptTemplate.FindVariableName(subPerception) ?? GenerateCodeForPullPerception(subPerception, scriptTemplate);
                        if (subPerceptionName != null) args.Add(subPerceptionName);
                    }
                    methodCode = $"new {cpa.perception.TypeName()}({args.Join()})";
                }
            }
            else if(perception is StatusPerceptionAsset spa)
            {
                var statusPerception = spa.perception as ExecutionStatusPerception;
                if(statusPerception != null)
                {
                    methodCode = $"new {nameof(ExecutionStatusPerception)}(null /**/, {statusPerception.StatusFlags.ToCodeFormat()})";
                }
            }
            else
            {
                if(p is CustomPerception customPerception)
                {
                    var parameters = new List<string>();

                    var initMethodArg = GenerateSerializedMethodCode(customPerception.init, scriptTemplate);
                    var checkMethodArg = GenerateSerializedMethodCode(customPerception.check, scriptTemplate);
                    var resetMethodArg = GenerateSerializedMethodCode(customPerception.reset, scriptTemplate);

                    if (initMethodArg != null) parameters.Add(initMethodArg);
                    if (checkMethodArg != null) parameters.Add(checkMethodArg);
                    else parameters.Add("() => false");
                    if (resetMethodArg != null) parameters.Add(resetMethodArg);

                    methodCode = $"new {nameof(ConditionPerception)}({string.Join(", ", parameters)})";
                    type = typeof(ConditionPerception);
                }
                else if(p is UnityPerception unityPerception)
                {
                    methodCode = GenerateConstructorCode(unityPerception, scriptTemplate);
                }
            }

            if(!string.IsNullOrEmpty(methodCode))
            {
                var name = scriptTemplate.AddVariableDeclarationLine(type, perception.Name, perception, methodCode);
                return name;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generates code for a action inline.
        /// new ACTIONTYPE(args)
        /// </summary>
        static string GenerateActionCode(Action action, ScriptTemplate scriptTemplate)
        {
            if (action is CustomAction customAction)
            {
                var parameters = new List<string>();

                var startMethodArg = GenerateSerializedMethodCode(customAction.start, scriptTemplate);
                var updateMethodArg = GenerateSerializedMethodCode(customAction.update, scriptTemplate);
                var stopMethodArg = GenerateSerializedMethodCode(customAction.stop, scriptTemplate);

                if (startMethodArg != null) parameters.Add(startMethodArg);
                if (updateMethodArg != null) parameters.Add(updateMethodArg);
                else parameters.Add("() => Status.Running");
                if (stopMethodArg != null) parameters.Add(stopMethodArg);

                return $"new {nameof(FunctionalAction)}({string.Join(", ", parameters)})";

            }
            else if (action is UnityAction unityAction)
            {
                scriptTemplate.AddUsingDirective(typeof(UnityAction).Namespace);
                return GenerateConstructorCode(unityAction, scriptTemplate);
            }
            else if (action is SubgraphAction subgraphAction)
            {
                var graphName = scriptTemplate.FindVariableName(subgraphAction.Subgraph);
                return $"new {nameof(SubsystemAction)}({graphName ?? "null /* Missing subgraph */"})";
            }
            else
                return null;
        }

        /// <summary>
        /// Generates code for a perception variable.
        /// PERCEPTIONTYPE p = new PERCEPTIONTYPE(args);
        static string GeneratePerceptionCode(Perception perception, ScriptTemplate scriptTemplate)
        {
            if (perception is CustomPerception customPerception)
            {
                var parameters = new List<string>();

                var initMethodArg = GenerateSerializedMethodCode(customPerception.init, scriptTemplate);
                var checkMethodArg = GenerateSerializedMethodCode(customPerception.check, scriptTemplate);
                var resetMethodArg = GenerateSerializedMethodCode(customPerception.reset, scriptTemplate);

                if (initMethodArg != null) parameters.Add(initMethodArg);
                if (checkMethodArg != null) parameters.Add(checkMethodArg);
                else parameters.Add("() => false");
                if (resetMethodArg != null) parameters.Add(resetMethodArg);

                return $"new {nameof(ConditionPerception)}({string.Join(", ", parameters)})";
            }
            else if (perception is UnityPerception unityPerception)
            {
                return GenerateConstructorCode(unityPerception, scriptTemplate);
            }
            else
                return null;
        }

        #endregion


        #region ------------------------------------------ Graphs ------------------------------------------
        /// <summary>
        /// Add all the instructions to create the graph nodes and connections in the code.
        /// </summary>
        static void GenerateCodeForGraph(GraphAsset graphAsset, ScriptTemplate scriptTemplate, bool includeNodeName)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            var graph = graphAsset.Graph;

            scriptTemplate.AddLine($"// {graphName}:");
            if (graph is FSM fsm)
            {
                var states = graphAsset.Nodes.FindAll(n => n.Node is State);
                var transitions = graphAsset.Nodes.FindAll(n => n.Node is Transition);

                foreach (var state in states)
                {
                    GenerateCodeForState(state, scriptTemplate, graphName, includeNodeName);
                }
                scriptTemplate.AddLine("");

                foreach (var transition in transitions)
                {
                    GenerateCodeForTransition(transition, scriptTemplate, graphName, includeNodeName);
                }

                if(states.Count > 0)
                {
                    var entryStateName = scriptTemplate.FindVariableName(states[0]);
                    if(entryStateName != null)
                    {
                        scriptTemplate.AddLine($"{graphName}.SetEntryState({entryStateName});");
                    }
                }
            }
            else if(graph is BehaviourTree bt)
            {
                foreach(var btNodeAsset in graphAsset.Nodes)
                {
                    if(scriptTemplate.FindVariableName(btNodeAsset) == null)
                    {
                        GenerateCodeForBTNode(btNodeAsset, scriptTemplate, graphName, includeNodeName);
                    }
                }

                if(graphAsset.Nodes.Count > 0)
                {
                    var rootName = scriptTemplate.FindVariableName(graphAsset.Nodes[0]);
                    if(rootName != null)
                    {
                        scriptTemplate.AddLine($"{graphName}.SetRootNode({rootName});");
                    }
                }
            }
            else if(graph is UtilitySystem us)
            {
                var selectables = graphAsset.Nodes.FindAll(n => n.Node is UtilitySelectableNode);
                var factors = graphAsset.Nodes.FindAll(n => n.Node is Factor);

                foreach(var factorAsset in factors)
                {
                    if(scriptTemplate.FindVariableName(factorAsset) == null)
                    {
                        GenerateCodeForFactor(factorAsset, scriptTemplate, graphName, includeNodeName);
                    }
                }

                scriptTemplate.AddLine("");

                foreach(var selectableAsset in selectables)
                {
                    if (scriptTemplate.FindVariableName(selectableAsset) == null)
                    {
                        GenerateCodeForSelectableNode(selectableAsset, scriptTemplate, graphName, includeNodeName);
                    }
                }
            }
        }
        #endregion

        #region --------------------------------------- UtilitySystems ---------------------------------------

        /// <summary>
        /// Generates code for a utility system's factor. If the factor has childs, the method generates code recursively.
        /// FACTORTYPE VARNAME = USNAME.CreateFACTORTYPE(args).SetVARNAME(VARVALUE)...;
        /// </summary>
        static string GenerateCodeForFactor(NodeAsset asset, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var factor = asset.Node as Factor;

            if(factor == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : factor.TypeName().ToLower();
            string typeName = factor.TypeName();

            var methodCode = "";
            var args = new List<string>();

            if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

            // If is a variable factor, generates code for the serialized method.
            if (factor is VariableFactor variableFactor)
            {
                var functionCode = GenerateSerializedMethodCode(variableFactor.variableFunction, template) ?? "() => 0f /*Missing function*/";
                args.Add(functionCode);
                args.Add(variableFactor.min.ToCodeFormat());
                args.Add(variableFactor.max.ToCodeFormat());
                methodCode = $"CreateVariableFactor({args.Join()})";
            }
            // If is a function factor, generates also code for the child if wasn't generated yet. The generates code for the setters.
            else if (factor is FunctionFactor functionFactor)
            {
                var child = asset.Childs.FirstOrDefault();
                string childName = child != null ? template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName, includeNodeName) : k_CodeForMissingNode;
                args.Add(childName);
                string setterCode = GenerateSetterCode(functionFactor, template);
                methodCode = $"CreateFunctionFactor<{typeName}>({args.Join()}){setterCode}";
            }
            // If is a fusion factor, generates also code for all its children if wasn't generated yet. Also generates code for the weights if necessary.
            else if (factor is FusionFactor fusionFactor)
            {
                foreach (var child in asset.Childs)
                {
                    var childName = template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName, includeNodeName);
                    if (childName != null) args.Add(childName);
                }
                methodCode = $"CreateFusionFactor<{typeName}>({args.Join()})";

                if (fusionFactor is WeightedFusionFactor weightedFusionFactor)
                {
                    var weightArgs = weightedFusionFactor.Weights.Select(w => w.ToCodeFormat());
                    if (weightArgs.Count() > 0) methodCode += $".SetWeights({weightArgs.Join()})";
                }
            }
            return template.AddVariableDeclarationLine(factor.GetType(), nodeName, asset, $"{graphName}.{methodCode}");
        }

        /// <summary>
        /// Generates code for an Utility Selectable Node. If the node has childs, generates code recursively.
        /// NODETYPE VARNAME = USNAME.CreateNODETYPE(args)...;
        /// </summary>
        static string GenerateCodeForSelectableNode(NodeAsset asset, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            UtilitySelectableNode selectableNode = asset.Node as UtilitySelectableNode;

            if (selectableNode == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : selectableNode.TypeName().ToLower();

            var method = "";
            var args = new List<string>();

            if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

            // If is an utility action, generates code for the child factor and for the action if necessary.
            if (selectableNode is UtilityAction action)
            {
                args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);

                if (action.Action != null) args.Add(GenerateActionCode(action.Action, template));
                if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
                method = $"CreateAction";
            }
            // If is an utility exit node, generates code for the child factor and the exit status.
            else if (selectableNode is UtilityExitNode exitNode)
            {
                args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);
                args.Add(exitNode.ExitStatus.ToCodeFormat());
                method = $"CreateExitNode";
            }
            // If is an utility bucket, only generates code for the variables
            else if (selectableNode is UtilityBucket bucket)
            {
                args.Add($"{bucket.Inertia.ToCodeFormat()}");
                args.Add($"{bucket.BucketThreshold.ToCodeFormat()}");
                method = $"CreateBucket";
            }

            // If the element is part of a group, the group must be created before it
            if (asset.Parents.Count > 0)
            {
                var groupAsset = asset.Parents.First();
                string group = template.FindVariableName(groupAsset) ?? GenerateCodeForSelectableNode(asset.Parents.First(), template, graphName, includeNodeName);
                args.Add("group: " + group);
            }

            return template.AddVariableDeclarationLine(selectableNode.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
        
        
        }
        #endregion

        #region ----------------------------------------- BTNodes -----------------------------------------

        /// <summary>
        /// Generates code for an BTNode. If the node has childs, generates code recursively.
        /// NODETYPE VARNAME = USNAME.CreateNODETYPE(args)...;
        /// </summary>
        static string GenerateCodeForBTNode(NodeAsset asset, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var btNode = asset.Node as BTNode;

            if(btNode == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = asset.Name ?? btNode.TypeName().ToLower();
            var typeName = btNode.TypeName();

            var method = "";
            var args = new List<string>();

            if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

            if (btNode is CompositeNode composite)
            {
                args.Add(composite.IsRandomized.ToCodeFormat());

                foreach (var child in asset.Childs)
                {
                    var childName = GenerateCodeForBTNode(child, template, graphName, includeNodeName);
                    if (childName != null) args.Add(childName);
                }
                method = $"CreateComposite<{typeName}>({args.Join()})";
            }
            else if (btNode is DecoratorNode decorator)
            {
                var childName = GenerateCodeForBTNode(asset.Childs.FirstOrDefault(), template, graphName, includeNodeName) ?? "null /* Missing child */";
                args.Add(childName);

                var setterCode = GenerateSetterCode(decorator, template);
                method = $"CreateDecorator<{typeName}>({args.Join()}){setterCode}";
                if (btNode is Framework.Adaptations.ConditionNode cn)
                {
                    var perceptionName = template.FindVariableName(cn.perception) ?? "null /*Missing perception*/";
                    method += $".SetPerception({perceptionName})";
                }
            }
            else if (btNode is LeafNode leaf)
            {
                var actionCode = GenerateActionCode(leaf.Action, template) ?? "null /* Missing action */";
                args.Add(actionCode);
                method = $"CreateLeafNode({args.Join()})";
            }

            return template.AddVariableDeclarationLine(btNode.GetType(), nodeName, asset, $"{graphName}.{method}");
        }

        #endregion

        #region ------------------------------------------- FSMs ------------------------------------------
        /// <summary>
        /// 
        /// </summary>

        static string GenerateCodeForState(NodeAsset asset, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var state = asset.Node as State;

            if (state == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : state.TypeName().ToLower();
            var typeName = state.TypeName();

            var args = new List<string>();
            if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

            var actionCode = GenerateActionCode(state.Action, template);
            if(!string.IsNullOrEmpty(actionCode)) args.Add(actionCode);
            return template.AddVariableDeclarationLine(state.GetType(), nodeName, asset, $"{graphName}.CreateState({args.Join()})");
        }

        /// <summary>
        /// 
        /// </summary>
        static string GenerateCodeForTransition(NodeAsset asset, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var transition = asset.Node as Transition;

            if (transition == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = asset.Name ?? transition.TypeName().ToLower();
            var method = "";
            

            bool perceptionAdded = false;

            var args = new List<string>();

            if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

            var sourceState = template.FindVariableName(asset.Parents.FirstOrDefault()) ?? "null/*ERROR*/";
            args.Add(sourceState);

            var perceptionCode = "";

            if (transition is StateTransition stateTransition)
            {
                var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                args.Add(targetState);                

                if(stateTransition is Framework.Adaptations.StateTransition sta)
                {
                    perceptionCode = template.FindVariableName(sta.perception);
                }
               
                method = "CreateTransition";
            }
            else if (transition is ExitTransition exitTransition)
            {
                args.Add(exitTransition.ExitStatus.ToCodeFormat());


                if (exitTransition is Framework.Adaptations.ExitTransition sta)
                {
                    perceptionCode = template.FindVariableName(sta.perception);
                }

                method = "CreateExitTransition";
            }
            else if (transition is PopTransition popTransition)
            {
                if (popTransition is Framework.Adaptations.PopTransition sta)
                {
                    perceptionCode = template.FindVariableName(sta.perception);
                }

                method = "CreatePopTransition";
            }
            else if (transition is PushTransition pushTransition)
            {
                var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                args.Add(targetState);

                if (pushTransition is Framework.Adaptations.PushTransition sta)
                {
                    perceptionCode = template.FindVariableName(sta.perception);
                }

                method = "CreatePushTransition";
            }

            if (!string.IsNullOrEmpty(perceptionCode))
            {
                args.Add(perceptionCode);
                perceptionAdded = true;
            }

            if (transition.Action != null)
            {
                var actionCode = GenerateActionCode(transition.Action, template);
                if (!string.IsNullOrEmpty(actionCode))
                {
                    if (!perceptionAdded) actionCode = "action: " + actionCode;
                    args.Add(actionCode);
                }
            }

            if (transition.StatusFlags != StatusFlags.Actived) args.Add($"statusFlags: {transition.StatusFlags.ToCodeFormat()}");

            return template.AddVariableDeclarationLine(transition.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
        }

        #endregion

        #region ---------------------------------------- Other elements ----------------------------------------

        /// <summary>
        /// Generates a constructor code for a class. 
        /// First search for the first constructor with at least one parameter.
        /// If not found, uses a parameterless constructor.
        /// The code will throw an exception at compilation time if the class has not
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="scriptTemplate"></param>
        /// <returns></returns>
        static string GenerateConstructorCode(object obj, ScriptTemplate scriptTemplate)
        {
            var type = obj.GetType();
            var constructor = type.GetConstructors().FirstOrDefault(f => f.GetParameters().Length != 0);

            var args = new List<string>();

            if (constructor != null)
            {
                var parameters = constructor.GetParameters();
                foreach (var param in parameters)
                {
                    var field = type.GetField(param.Name);
                    if (field != null && field.FieldType == param.ParameterType)
                    {
                        args.Add(scriptTemplate.AddVariableDeclaration(field.FieldType, field.Name, field.GetValue(obj)));
                    }
                }

                if (parameters.Count() != args.Count) args.Clear();
            }
            return $"new {obj.TypeName()}({args.Join()})";
        }

        /// <summary>
        /// Generates code for a serialized method used in actions, perceptions and factors.
        /// COMPONENT.METHOD
        static string GenerateSerializedMethodCode(SerializedMethod serializedMethod, ScriptTemplate scriptTemplate)
        {
            if (serializedMethod != null && serializedMethod.component != null && !string.IsNullOrEmpty(serializedMethod.methodName))
            {
                var component = serializedMethod.component;
                var componentName = scriptTemplate.
                    AddVariableDeclaration(component.GetType(), component.TypeName().ToLower(), component);
                return $"{componentName}.{serializedMethod.methodName}";
            }
            else return null;
        }

        /// <summary>
        /// Generates code for properties initialized with setter methods.
        /// .SetVARNAME(VARVALUE).Set...
        /// </summary>
        static string GenerateSetterCode(Node node, ScriptTemplate scriptTemplate)
        {
            var type = node.GetType();
            var fields = type.GetFields();
            var functionCode = "";
            foreach (var field in fields)
            {
                var methodName = $"Set{field.Name}";
                var method = type.GetMethod(methodName);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Count() == 1 && parameters[0].ParameterType == field.FieldType)
                    {
                        var argCode = "";
                        if(field.FieldType.IsAssignableFrom(typeof(Action)))
                        {
                            argCode = GenerateActionCode(field.GetValue(node) as Action, scriptTemplate);
                        }
                        else if(field.FieldType.IsAssignableFrom(typeof(Perception)))
                        {
                            break;
                        }                       
                        else
                        {
                            argCode = scriptTemplate.AddVariableDeclaration(parameters[0].ParameterType, field.Name, field.GetValue(node));
                        }
                        functionCode += $".{methodName}({argCode})";
                    }
                }
            }
            return functionCode;
        }
        #endregion
    }
}

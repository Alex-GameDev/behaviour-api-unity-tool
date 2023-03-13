using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;

using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.StateMachines;

using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.Unity.Runtime;
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


using UtilityAction = BehaviourAPI.UtilitySystems.UtilityAction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;
using StateTransition = BehaviourAPI.StateMachines.StateTransition;
using PopTransition = BehaviourAPI.StateMachines.StackFSMs.PopTransition;
using PushTransition = BehaviourAPI.StateMachines.StackFSMs.PushTransition;
using ProbabilisticState = BehaviourAPI.StateMachines.ProbabilisticState;
using behaviourAPI.Unity.Framework.Adaptations;
using BehaviourAPI.UnityExtensions;
using LeafNode = BehaviourAPI.Unity.Framework.Adaptations.LeafNode;
using System;
using BehaviourAPI.UtilitySystems.Factors;
using UnityEditor.VersionControl;

namespace BehaviourAPI.Unity.Editor
{
    public static class ScriptGeneration
    {
        private static readonly string[] k_basicNamespaces =
        {
            "System",
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
        /// Create a script for a<see cref="BehaviourRunner"/> with all the data stored in <paramref name = "asset" />
        /// </ summary >
        /// < param name="path">The destination PATH to the script.</param>
        /// <param name = "name" > The script class name.</param>
        /// <param name = "asset" > The system asset.</param>
        public static void GenerateScript(string path, string name, IBehaviourSystem asset, bool useFullNameVar = true, bool includeNodeNames = true)
        {
            if (asset == null)
            {
                Debug.LogWarning("Cant generate code of a null system");
                return;
            }

            var scriptPath = $"{path}{name}.cs";
            if (!string.IsNullOrEmpty(scriptPath))
            {
                Object obj = CreateScript(scriptPath, asset, useFullNameVar, includeNodeNames);
                AssetDatabase.Refresh();
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }

        static Object CreateScript(string path, IBehaviourSystem asset, bool useFullNameVar, bool includeNodeNames)
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
        static string GenerateSystemCode(IBehaviourSystem asset, string scriptName, bool useFullNameVar, bool includeNames)
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

            foreach (var graphAsset in asset.Data.graphs)
            {
                var graph = graphAsset.graph;
                var graphName = graphAsset.name;

                if (graph == null)
                {
                    Debug.LogWarning($"Graph {graphName} is empty.");
                    scriptTemplate.AddLine($"//Graph {graphName} is empty.");
                    continue;
                }

                if (string.IsNullOrEmpty(graphName)) graphName = graph.TypeName().ToLower();

                graphName = AddCreateGraphLine(graphAsset, graphName, scriptTemplate);
            }

            // Nodes
            scriptTemplate.AddLine("");
            scriptTemplate.AddLine("/* --------------------------- Nodes: --------------------------- */");
            scriptTemplate.AddLine("");

            foreach (var graphAsset in asset.Data.graphs)
            {
                GenerateCodeForGraph(graphAsset, scriptTemplate, includeNames);
                scriptTemplate.AddLine("");
            }

            // Push perceptions
            scriptTemplate.AddLine("");
            scriptTemplate.AddLine("/* ------------------------- Push Perceptions: ------------------------- */");
            scriptTemplate.AddLine("");

            foreach (var pushPerception in asset.Data.pushPerceptions)
            {
                GenerateCodeForPushPerception(pushPerception, scriptTemplate);
            }

            // Return
            if (asset.Data.graphs.Count > 0)
            {
                var mainGraphName = scriptTemplate.FindVariableName(asset.Data.graphs[0]);
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
        static string AddCreateGraphLine(GraphData graphAsset, string graphName, ScriptTemplate template)
        {
            var graph = graphAsset.graph;
            var type = graph.GetType();

            template.RegisterGraph(graphAsset);
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
        /// <param name="template">YThe script template</param>
        private static void GenerateCodeForPushPerception(PushPerceptionData pushPerception, ScriptTemplate template)
        {

            var targets = pushPerception.targetNodeIds.Select(id => template.FindNode(id)).ToList().FindAll(tgt => tgt.node is IPushActivable);  
            template.AddVariableInstantiationLine(typeof(PushPerception), pushPerception.name, pushPerception, targets);
        }       

        /// <summary>
        /// Generates code for a action inline.
        /// new ACTIONTYPE(args)
        /// </summary>
        static string GenerateActionCode(Action action, ScriptTemplate scriptTemplate)
        {
            switch (action)
            {
                case CustomAction custom:
                    List<string> actionParameters = new List<string>();

                    string startMethodArg = GenerateMethodCode(custom.start, scriptTemplate, null);
                    string updateMethodArg = GenerateMethodCode(custom.update, scriptTemplate, null, typeof(Status));
                    string stopMethodArg = GenerateMethodCode(custom.stop, scriptTemplate, null);

                    if (startMethodArg != null) actionParameters.Add(startMethodArg);

                    if (updateMethodArg != null) actionParameters.Add(updateMethodArg);
                    else actionParameters.Add("() => Status.Running");

                    if (stopMethodArg != null) actionParameters.Add(stopMethodArg);

                    return $"new {nameof(FunctionalAction)}({string.Join(", ", actionParameters)})";

                case UnityAction unityAction:
                    return GenerateConstructorCode(unityAction, scriptTemplate);

                case SubgraphAction subgraphAction:
                    var subgraphName = scriptTemplate.FindGraphVarName(subgraphAction.subgraphId);
                    return $"new {nameof(SubsystemAction)}({subgraphName ?? "null /* Missing subgraph */"})";

                default:
                    return null;
            }
        }

        static string GeneratePerceptionCode(Perception perception, ScriptTemplate template)
        {
            switch (perception)
            {
                case CustomPerception custom:
                    List<string> perceptionParameters = new List<string>();

                    string initMethodArg = GenerateMethodCode(custom.init, template, null);
                    string checkMethodArg = GenerateMethodCode(custom.check, template, null, typeof(Status));
                    string resetMethodArg = GenerateMethodCode(custom.reset, template, null);

                    if (initMethodArg != null) perceptionParameters.Add(initMethodArg);

                    if (checkMethodArg != null) perceptionParameters.Add(checkMethodArg);
                    else perceptionParameters.Add("() => false");

                    if (resetMethodArg != null) perceptionParameters.Add(resetMethodArg);

                    return $"new {nameof(ConditionPerception)}({string.Join(", ", perceptionParameters)})";

                case UnityPerception unityPerception:
                    return GenerateConstructorCode(unityPerception, template);

                case CompoundPerceptionWrapper compound:
                    if (compound.compoundPerception != null)
                    {
                        IEnumerable<string> subPerceptionvariableNames = compound.subPerceptions.Select(sub => GeneratePerceptionCode(sub.perception, template));
                        return $"new {compound.compoundPerception.TypeName()}({subPerceptionvariableNames.Join()})";
                    }
                    else return null;

                default:
                    return null;
            }
        }


        #endregion


        #region ------------------------------------------ Graphs ------------------------------------------
        /// <summary>
        /// Add all the instructions to create the graph nodes and connections in the code.
        /// </summary>
        static void GenerateCodeForGraph(GraphData graphAsset, ScriptTemplate scriptTemplate, bool includeNodeName)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            var graph = graphAsset.graph;

            scriptTemplate.AddLine($"// {graphName}:");
            if (graph is FSM fsm)
            {
                var states = graphAsset.nodes.FindAll(n => n.node is State);
                var transitions = graphAsset.nodes.FindAll(n => n.node is Transition);

                foreach (var state in states)
                {
                    //GenerateCodeForState(state, scriptTemplate, graphName, includeNodeName);
                }
                scriptTemplate.AddLine("");

                foreach (var transition in transitions)
                {
                    //GenerateCodeForTransition(transition, scriptTemplate, graphName, includeNodeName);
                }

                if (states.Count > 0)
                {
                    var entryStateName = scriptTemplate.FindVariableName(states[0]);
                    if (entryStateName != null)
                    {
                        scriptTemplate.AddLine($"{graphName}.SetEntryState({entryStateName});");
                    }
                }
            }
            else if (graph is BehaviourTree bt)
            {
                foreach (var btNodeAsset in graphAsset.nodes)
                {
                    if (scriptTemplate.FindVariableName(btNodeAsset) == null)
                    {
                        GenerateCodeForBTNode(btNodeAsset, scriptTemplate, graphName, includeNodeName);
                    }
                }

                if (graphAsset.nodes.Count > 0)
                {
                    var rootName = scriptTemplate.FindVariableName(graphAsset.nodes[0]);
                    if (rootName != null)
                    {
                        scriptTemplate.AddLine($"{graphName}.SetRootNode({rootName});");
                    }
                }
            }
            else if (graph is UtilitySystem us)
            {
                var selectables = graphAsset.nodes.FindAll(n => n.node is UtilitySelectableNode);
                var factors = graphAsset.nodes.FindAll(n => n.node is Factor);

                foreach (var factorAsset in factors)
                {
                    if (scriptTemplate.FindVariableName(factorAsset) == null)
                    {
                        //GenerateCodeForFactor(factorAsset, scriptTemplate, graphName, includeNodeName);
                    }
                }

                scriptTemplate.AddLine("");

                foreach (var selectableAsset in selectables)
                {
                    if (scriptTemplate.FindVariableName(selectableAsset) == null)
                    {
                        //GenerateCodeForSelectableNode(selectableAsset, scriptTemplate, graphName, includeNodeName);
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
        //static string GenerateCodeForFactor(NodeData asset, ScriptTemplate template, string graphName, bool includeNodeName)
        //{
        //    var factor = asset.node as Factor;

        //    if (factor == null)
        //    {
        //        return k_CodeForMissingNode;
        //    }

        //    var nodeName = !string.IsNullOrEmpty(asset.name) ? asset.name : factor.TypeName().ToLower();
        //    string typeName = factor.TypeName();

        //    var methodCode = "";
        //    var args = new List<string>();

        //    if (includeNodeName && !string.IsNullOrEmpty(asset.name)) args.Add($"\"{asset.name}\"");

        //    // If is a variable factor, generates code for the serialized method.
        //    if (factor is VariableFactor variableFactor)
        //    {
        //        var functionCode = GenerateMethodCode(variableFactor.variableFunction, template, null, typeof(float)) ?? "() => 0f /*Missing function*/";
        //        args.Add(functionCode);
        //        args.Add(variableFactor.min.ToCodeFormat());
        //        args.Add(variableFactor.max.ToCodeFormat());
        //        methodCode = $"CreateVariableFactor({args.Join()})";
        //    }
        //    else if(factor is ConstantFactor constantFactor)
        //    {
        //        args.Add(constantFactor.value.ToCodeFormat());
        //        methodCode = $"CreateConstantFactor({args.Join()})";
        //    }
        //    // If is a function factor, generates also code for the child if wasn't generated yet. The generates code for the setters.
        //    else if (factor is CurveFactor functionFactor)
        //    {
        //        var child = template.FindNode(asset.childIds.FirstOrDefault());
        //        string childName = child != null ? template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName, includeNodeName) : k_CodeForMissingNode;
        //        args.Add(childName);
        //        string setterCode = GenerateSetterCode(functionFactor, template);
        //        methodCode = $"CreateCurveFactor<{typeName}>({args.Join()}){setterCode}";
        //    }
        //    // If is a fusion factor, generates also code for all its children if wasn't generated yet. Also generates code for the weights if necessary.
        //    else if (factor is FusionFactor fusionFactor)
        //    {
        //        foreach (var childId in asset.childIds)
        //        {
        //            var child = template.FindNode(asset.childIds.FirstOrDefault());
        //            var childName = template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName, includeNodeName);
        //            if (childName != null) args.Add(childName);
        //        }
        //        methodCode = $"CreateFusionFactor<{typeName}>({args.Join()})";

        //        if (fusionFactor is WeightedFusionFactor weightedFusionFactor)
        //        {
        //            var weightArgs = weightedFusionFactor.Weights.Select(w => w.ToCodeFormat());
        //            if (weightArgs.Count() > 0) methodCode += $".SetWeights({weightArgs.Join()})";
        //        }
        //    }
        //    return template.AddVariableDeclarationLine(factor.GetType(), nodeName, asset, $"{graphName}.{methodCode}");
        //}

        ///// <summary>
        ///// Generates code for an Utility Selectable Node. If the data has childs, generates code recursively.
        ///// NODETYPE VARNAME = USNAME.CreateNODETYPE(args)...;
        ///// </summary>
        //static string GenerateCodeForSelectableNode(NodeData asset, ScriptTemplate template, string graphName, bool includeNodeName)
        //{
        //    UtilitySelectableNode selectableNode = asset.node as UtilitySelectableNode;

        //    if (selectableNode == null)
        //    {
        //        return k_CodeForMissingNode;
        //    }

        //    var nodeName = !string.IsNullOrEmpty(asset.name) ? asset.name : selectableNode.TypeName().ToLower();

        //    var method = "";
        //    var args = new List<string>();

        //    if (includeNodeName && !string.IsNullOrEmpty(asset.name)) args.Add($"\"{asset.name}\"");

        //    // If is an utility action, generates code for the child factor and for the action if necessary.
        //    if (selectableNode is UtilityAction action)
        //    {
        //        args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);

        //        if (action.Action != null) args.Add(GenerateActionCode(action as IActionAssignable, template));
        //        if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
        //        method = $"CreateAction";
        //    }
        //    // If is an utility exit data, generates code for the child factor and the exit status.
        //    else if (selectableNode is UtilityExitNode exitNode)
        //    {
        //        args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);
        //        args.Add(exitNode.ExitStatus.ToCodeFormat());
        //        method = $"CreateExitNode";
        //    }
        //    // If is an utility bucket, only generates code for the variables
        //    else if (selectableNode is UtilityBucket bucket)
        //    {
        //        args.Add($"{bucket.Inertia.ToCodeFormat()}");
        //        args.Add($"{bucket.BucketThreshold.ToCodeFormat()}");
        //        method = $"CreateBucket";
        //    }

        //    // If the element is part of a group, the group must be created before it
        //    if (asset.parentIds.Count > 0)
        //    {
        //        var groupAsset = asset.Parents.First();
        //        string group = template.FindVariableName(groupAsset) ?? GenerateCodeForSelectableNode(asset.Parents.First(), template, graphName, includeNodeName);
        //        args.Add("group: " + group);
        //    }

        //    return template.AddVariableDeclarationLine(selectableNode.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
        //}
        #endregion

        #region ----------------------------------------- BTNodes -----------------------------------------

        /// <summary>
        /// Generates code for an BTNode. If the data has childs, generates code recursively.
        /// NODETYPE VARNAME = USNAME.CreateNODETYPE(args)...;
        /// </summary>
        static string GenerateCodeForBTNode(NodeData data, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            switch (data.node)
            {
                case LeafNode leafNode: return GenerateLeafNodeCode(data, leafNode, template, graphName, includeNodeName);
                case DecoratorNode decoratorNode: return GenerateDecoratorNodeCode(data, decoratorNode, template, graphName, includeNodeName);
                case CompositeNode compositeNode: return GenerateCompositeNodeCode(data, compositeNode, template, graphName, includeNodeName);
                default: return null;
            }                  
        }

        static string GenerateLeafNodeCode(NodeData nodeData, LeafNode leafNode, ScriptTemplate template, string graphName, bool includeNodeName)
        {

            Action action = leafNode.ActionReference;
            string actionVariableName = GenerateActionCode(action, template);
            return template.AddVariableDeclarationLine(typeof(LeafNode), nodeData.name, nodeData,
                $"{graphName}.CreateLeafNode({actionVariableName ?? "missing action"});");
        }

        static string GenerateDecoratorNodeCode(NodeData nodeData, DecoratorNode decoratorNode, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var child = template.FindNode(nodeData.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForBTNode(child, template, graphName, includeNodeName) : k_CodeForMissingNode;

            return template.AddVariableDeclarationLine(decoratorNode.GetType(), nodeData.name, nodeData,
                $"{graphName}.CreateDecorator<{decoratorNode.TypeName()}>({childName ?? "missing action"});");
        }
        static string GenerateCompositeNodeCode(NodeData nodeData, CompositeNode compositeNode, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();
            args.Add(compositeNode.IsRandomized.ToCodeFormat());

            for(int i = 0; i < nodeData.childIds.Count; i++)
            {
                var child = template.FindNode(nodeData.childIds[i]);
                if(child != null) args.Add(template.FindVariableName(child) ?? GenerateCodeForBTNode(child, template, graphName, includeNodeName));
            }

            return template.AddVariableDeclarationLine(compositeNode.GetType(), nodeData.name, nodeData,
                $"{graphName}.CreateComposite<{compositeNode.TypeName()}>({args.Join()})");
        }


        #endregion

        #region ------------------------------------------- FSMs ------------------------------------------

        //static string GenerateCodeForState(NodeData asset, ScriptTemplate template, string graphName, bool includeNodeName)
        //{

        //    switch(asset.node)
        //    {
        //        case Framework.Adaptations.State:

        //                return template.AddVariableDeclarationLine
        //    }
        //    var state = asset.node as Framework.AdaptationsState;

        //    if (state == null)
        //    {
        //        return k_CodeForMissingNode;
        //    }

        //    var nodeName = !string.IsNullOrEmpty(asset.name) ? asset.name : state.TypeName().ToLower();
        //    var args = new List<string>();

        //    string actionCode = GenerateActionCode(state.Action, template);
        //    string method = "CreateState";
        //    if (state is ProbabilisticState)
        //    {
        //        method += "<ProbabilisticState>";
        //    }

        //    if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

        //    if (!string.IsNullOrEmpty(actionCode)) args.Add(actionCode);
        //    return template.AddVariableDeclarationLine(state.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
        //}

        //static string GenerateCodeForTransition(NodeData asset, ScriptTemplate template, string graphName, bool includeNodeName)
        //{
        //    var transition = asset.Node as Transition;

        //    if (transition == null)
        //    {
        //        return k_CodeForMissingNode;
        //    }

        //    var nodeName = asset.Name ?? transition.TypeName().ToLower();
        //    var method = "";


        //    bool perceptionAdded = false;

        //    var args = new List<string>();

        //    if (includeNodeName && !string.IsNullOrEmpty(asset.Name)) args.Add($"\"{asset.Name}\"");

        //    var sourceState = template.FindVariableName(asset.Parents.FirstOrDefault()) ?? "null/*ERROR*/";
        //    args.Add(sourceState);

        //    var perceptionCode = "";

        //    if (transition is StateTransition stateTransition)
        //    {
        //        var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
        //        args.Add(targetState);

        //        if (stateTransition is Framework.Adaptations.StateTransition sta)
        //        {
        //            perceptionCode = template.FindVariableName(sta.PerceptionReference);
        //        }

        //        method = "CreateTransition";
        //    }
        //    else if (transition is ExitTransition exitTransition)
        //    {
        //        args.Add(exitTransition.ExitStatus.ToCodeFormat());


        //        if (exitTransition is Framework.Adaptations.ExitTransition sta)
        //        {
        //            perceptionCode = template.FindVariableName(sta.PerceptionReference);
        //        }

        //        method = "CreateExitTransition";
        //    }
        //    else if (transition is PopTransition popTransition)
        //    {
        //        if (popTransition is Framework.Adaptations.PopTransition sta)
        //        {
        //            perceptionCode = template.FindVariableName(sta.PerceptionReference);
        //        }

        //        method = "CreatePopTransition";
        //    }
        //    else if (transition is PushTransition pushTransition)
        //    {
        //        var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
        //        args.Add(targetState);

        //        if (pushTransition is Framework.Adaptations.PushTransition sta)
        //        {
        //            perceptionCode = template.FindVariableName(sta.PerceptionReference);
        //        }

        //        method = "CreatePushTransition";
        //    }

        //    if (!string.IsNullOrEmpty(perceptionCode))
        //    {
        //        args.Add(perceptionCode);
        //        perceptionAdded = true;
        //    }

        //    if (transition.Action != null)
        //    {
        //        var actionCode = GenerateActionCode(transition as IActionAssignable, template);
        //        if (!string.IsNullOrEmpty(actionCode))
        //        {
        //            if (!perceptionAdded) actionCode = "action: " + actionCode;
        //            args.Add(actionCode);
        //        }
        //    }

        //    if (transition.StatusFlags != StatusFlags.Actived) args.Add($"statusFlags: {transition.StatusFlags.ToCodeFormat()}");

        //    return template.AddVariableDeclarationLine(transition.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
        //}

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
                        if (field.FieldType.IsAssignableFrom(typeof(Action)))
                        {
                            break;
                        }
                        else if (field.FieldType.IsAssignableFrom(typeof(Perception)))
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

        static string GenerateMethodCode(SerializedContextMethod serializedMethod, ScriptTemplate template, Type[] args, Type returnType = null)
        {
            if (!string.IsNullOrWhiteSpace(serializedMethod.methodName))
            {
                return GenerateMethodCode(serializedMethod.componentName, serializedMethod.methodName, template, args, returnType);
            }
            else
            {
                return null;
            }
        }

        static string GenerateMethodCode(string componentName, string methodName, ScriptTemplate template, Type[] args, Type returnType = null)
        {
            Type componentType = string.IsNullOrWhiteSpace(componentName) ? null : TypeUtilities.FindComponentType(componentName);

            if (componentType != null)
            {
                var componentVariableName = template.AddComponentVariable(componentType);

                var methodInfo = componentType.GetMethod("methodName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null,
                    System.Reflection.CallingConventions.Any, args, null);

                if (methodInfo != null)
                {
                    return $"{componentVariableName}.{methodName}";
                }
                else
                {
                    return $"\"{componentVariableName}.{methodName} (Method not found)\"";
                }
            }
            else
            {
                var methodCallName = template.AddEmptyMethod(methodName, returnType);
                return methodCallName;
            }
        }
    }
}

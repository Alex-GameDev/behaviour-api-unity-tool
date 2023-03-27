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

using LeafNode = BehaviourAPI.Unity.Framework.Adaptations.LeafNode;

using UtilityAction = BehaviourAPI.Unity.Framework.Adaptations.UtilityAction;
using VariableFactor = BehaviourAPI.Unity.Framework.Adaptations.VariableFactor;

using PopTransition = BehaviourAPI.Unity.Framework.Adaptations.PopTransition;
using PushTransition = BehaviourAPI.Unity.Framework.Adaptations.PushTransition;
using ExitTransition = BehaviourAPI.Unity.Framework.Adaptations.ExitTransition;
using StateTransition = BehaviourAPI.Unity.Framework.Adaptations.StateTransition;

using State = BehaviourAPI.Unity.Framework.Adaptations.State;
using ProbabilisticState = BehaviourAPI.Unity.Framework.Adaptations.ProbabilisticState;

using BehaviourAPI.UnityExtensions;

using System;
using BehaviourAPI.StateMachines.StackFSMs;
using UnityEngine.Events;
using UnityAction = BehaviourAPI.UnityExtensions.UnityAction;

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
            "BehaviourAPI.Core.Perceptions",
            "BehaviourAPI.UnityExtensions"
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
        /// <param name = "asset" > The system data.</param>
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
        /// <param name="pushPerception">The push perception data</param>
        /// <param name="template">YThe script template</param>
        private static void GenerateCodeForPushPerception(PushPerceptionData pushPerception, ScriptTemplate template)
        {
            var targets = pushPerception.targetNodeIds.Select(id => template.FindNode(id)).ToList().FindAll(tgt => tgt.node is IPushActivable);
            template.AddVariableInstantiationLine(typeof(PushPerception), pushPerception.name, pushPerception, targets);
        }

        /// <summary>
        /// Generates code for a action.
        /// Action action = new ACTIONTYPE(args);
        /// </summary>
        static string GenerateActionCode(Action action, ScriptTemplate template, bool inline = false)
        {
            switch (action)
            {
                case CustomAction custom:
                    List<string> actionParameters = new List<string>();

                    string startMethodArg = GenerateMethodCode(custom.start, template, null);
                    string updateMethodArg = GenerateMethodCode(custom.update, template, null, typeof(Status));
                    string stopMethodArg = GenerateMethodCode(custom.stop, template, null);

                    if (startMethodArg != null) actionParameters.Add(startMethodArg);

                    if (updateMethodArg != null) actionParameters.Add(updateMethodArg);
                    else actionParameters.Add("() => Status.Running");

                    if (stopMethodArg != null) actionParameters.Add(stopMethodArg);

                    if (inline)
                    {
                        return $"new {nameof(FunctionalAction)}({string.Join(", ", actionParameters)})";
                    }
                    else
                    {
                        return template.AddVariableDeclarationLine(typeof(FunctionalAction), "action", action,
                        $"new {nameof(FunctionalAction)}({actionParameters.Join()})");
                    }

                case UnityAction unityAction:
                    var code = GenerateConstructorCode(unityAction, template);
                    if (inline)
                    {
                        return code;
                    }
                    else
                    {
                        return template.AddVariableDeclarationLine(unityAction.GetType(), "action", action, code);
                    }

                case SubgraphAction subgraphAction:
                    var subgraphName = template.FindGraphVarName(subgraphAction.subgraphId);
                    if (inline)
                    {
                        return $"new {nameof(SubsystemAction)}({subgraphName ?? "null /* Missing subgraph */"})";
                    }
                    else
                    {
                        return template.AddVariableDeclarationLine(typeof(SubsystemAction), "subAction", action,
                        $"new {nameof(SubsystemAction)}({subgraphName})");
                    }
                default:
                    return null;
            }
        }

        static string GeneratePerceptionCode(Perception perception, ScriptTemplate template, bool inline = false)
        {
            switch (perception)
            {
                case CustomPerception custom:
                    List<string> perceptionParameters = new List<string>();

                    string initMethodArg = GenerateMethodCode(custom.init, template, null);
                    string checkMethodArg = GenerateMethodCode(custom.check, template, null, typeof(bool));
                    string resetMethodArg = GenerateMethodCode(custom.reset, template, null);

                    if (initMethodArg != null) perceptionParameters.Add(initMethodArg);

                    if (checkMethodArg != null) perceptionParameters.Add(checkMethodArg);
                    else perceptionParameters.Add("() => false");

                    if (resetMethodArg != null) perceptionParameters.Add(resetMethodArg);

                    if (inline)
                    {
                        return $"new {nameof(ConditionPerception)}({string.Join(", ", perceptionParameters)})";
                    }
                    else
                    {
                        return template.AddVariableDeclarationLine(typeof(ConditionPerception), "perception", perception,
                       $"new {nameof(ConditionPerception)}({perceptionParameters.Join()})");
                    }
                case UnityPerception unityPerception:
                    var code = GenerateConstructorCode(unityPerception, template);
                    if (inline)
                    {
                        return GenerateConstructorCode(unityPerception, template);
                    }
                    else
                    {
                        return template.AddVariableDeclarationLine(unityPerception.GetType(), "perception", perception, code);
                    }

                case CompoundPerceptionWrapper compound:
                    if (compound.compoundPerception != null)
                    {
                        IEnumerable<string> subPerceptionvariableNames = compound.subPerceptions.Select(sub => GeneratePerceptionCode(sub.perception, template));

                        if (inline)
                        {
                            return $"new {compound.compoundPerception.TypeName()}({subPerceptionvariableNames.Join()})";
                        }
                        else
                        {
                            return template.AddVariableDeclarationLine(compound.compoundPerception.GetType(), "perception", perception,
                            $"new {compound.compoundPerception.TypeName()}({subPerceptionvariableNames.Join()})");
                        }
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
                    GenerateCodeForState(state, scriptTemplate, graphName, includeNodeName);
                }
                scriptTemplate.AddLine("");

                foreach (var transition in transitions)
                {
                    GenerateCodeForTransition(transition, scriptTemplate, graphName, includeNodeName);
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
                        //GenerateCodeForFactor(factorAsset, template, graphName, includeNodeName);
                    }
                }

                scriptTemplate.AddLine("");

                foreach (var selectableAsset in selectables)
                {
                    if (scriptTemplate.FindVariableName(selectableAsset) == null)
                    {
                        //GenerateCodeForSelectableNode(selectableAsset, template, graphName, includeNodeName);
                    }
                }
            }
        }
        #endregion

        #region --------------------------------------- UtilitySystems ---------------------------------------

        static string GenerateCodeForFactor(NodeData data, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            switch (data.node)
            {
                case VariableFactor variableFactor: return GenerateVariableFactorCode(data, variableFactor, template, graphName, includeNodeName);
                case ConstantFactor constantFactor: return GenerateConstantFactorCode(data, constantFactor, template, graphName, includeNodeName);
                case FusionFactor fusionFactor: return GenerateFusionFactorCode(data, fusionFactor, template, graphName, includeNodeName);
                case CurveFactor curveFactor: return GenerateCurveFactorCode(data, curveFactor, template, graphName, includeNodeName);
                default: return null;
            }
        }

        static string GenerateVariableFactorCode(NodeData data, VariableFactor variableFactor, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            string method = GenerateMethodCode(variableFactor.variableFunction, template, null, typeof(float));
            return template.AddVariableDeclarationLine(typeof(VariableFactor), data.name, data,
                $"{graphName}.CreateVariable({method})");
        }

        static string GenerateConstantFactorCode(NodeData data, ConstantFactor constantFactor, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            return template.AddVariableDeclarationLine(typeof(ConstantFactor), data.name, data,
                $"{graphName}.CreateConstant({constantFactor.value.ToCodeFormat()})");
        }

        static string GenerateCurveFactorCode(NodeData data, CurveFactor curveFactor, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            var child = template.FindNode(data.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForFactor(child, template, graphName, includeNodeName) : k_CodeForMissingNode;

            return template.AddVariableDeclarationLine(curveFactor.GetType(), data.name, data,
                $"{graphName}.CreateCurve<{curveFactor.TypeName()}>({childName})");
        }

        static string GenerateFusionFactorCode(NodeData data, FusionFactor fusionFactor, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();
            for (int i = 0; i < data.childIds.Count; i++)
            {
                var child = template.FindNode(data.childIds[i]);
                if (child != null) args.Add(template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName, includeNodeName));
            }

            return template.AddVariableDeclarationLine(fusionFactor.GetType(), data.name, data,
                $"{graphName}.CreateFusion<{fusionFactor.TypeName()}>({args.Join()})");
        }

        static string GenerateCodeForSelectableNode(NodeData data, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            switch (data.node)
            {
                case UtilityAction uAction: return GenerateUtilityActionCode(data, uAction, template, graphName, includeNodeName);
                case UtilityExitNode uExitNode: return GenerateUtilityExitNodeCode(data, uExitNode, template, graphName, includeNodeName);
                case UtilityBucket uBucket: return GenerateUtilityBucketCode(data, uBucket, template, graphName, includeNodeName);
                default: return null;
            }
        }

        static string GenerateUtilityBucketCode(NodeData data, UtilityBucket uBucket, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();

            args.Add(uBucket.Inertia.ToCodeFormat());
            args.Add(uBucket.BucketThreshold.ToCodeFormat());

            if (data.parentIds.Count > 0)
            {
                var parent = template.FindNode(data.parentIds.FirstOrDefault());
                if (parent.node is UtilityBucket)
                {
                    string group = template.FindVariableName(parent) ?? GenerateCodeForSelectableNode(parent, template, graphName, includeNodeName);
                    args.Add("group: " + group);
                }
            }

            return template.AddVariableDeclarationLine(typeof(UtilityBucket), data.name, data,
                $"{graphName}.CreateBucket({args.Join()})");
        }

        static string GenerateUtilityExitNodeCode(NodeData data, UtilityExitNode uExitNode, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();
            var child = template.FindNode(data.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForFactor(child, template, graphName, includeNodeName) : k_CodeForMissingNode;

            args.Add(childName);
            args.Add(uExitNode.ExitStatus.ToCodeFormat());

            if (data.parentIds.Count > 0)
            {
                var parent = template.FindNode(data.parentIds.FirstOrDefault());
                if (parent.node is UtilityBucket)
                {
                    string group = template.FindVariableName(parent) ?? GenerateCodeForSelectableNode(parent, template, graphName, includeNodeName);
                    args.Add("group: " + group);
                }
            }

            return template.AddVariableDeclarationLine(typeof(UtilityBucket), data.name, data,
                $"{graphName}.CreateExitNode({args.Join()})");
        }

        static string GenerateUtilityActionCode(NodeData data, UtilityAction uAction, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();
            var child = template.FindNode(data.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForFactor(child, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(childName);

            var actionCode = GenerateActionCode(uAction.ActionReference, template);
            args.Add(actionCode);

            if (uAction.FinishSystemOnComplete) args.Add("true");

            if (data.parentIds.Count > 0)
            {
                var parent = template.FindNode(data.parentIds.FirstOrDefault());
                if (parent.node is UtilityBucket)
                {
                    string group = template.FindVariableName(parent) ?? GenerateCodeForSelectableNode(parent, template, graphName, includeNodeName);
                    args.Add("group: " + group);
                }
            }

            return template.AddVariableDeclarationLine(typeof(UtilityAction), data.name, data,
                $"{graphName}.CreateAction({args.Join()})");
        }

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

            var name = template.AddVariableDeclarationLine(decoratorNode.GetType(), nodeData.name, nodeData,
                $"{graphName}.CreateDecorator<{decoratorNode.TypeName()}>({childName ?? "missing action"});");

            GenerateNodePropertiesCode(decoratorNode, name, template);
            return name;
        }
        static string GenerateCompositeNodeCode(NodeData nodeData, CompositeNode compositeNode, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();
            args.Add(compositeNode.IsRandomized.ToCodeFormat());

            for (int i = 0; i < nodeData.childIds.Count; i++)
            {
                var child = template.FindNode(nodeData.childIds[i]);
                if (child != null) args.Add(template.FindVariableName(child) ?? GenerateCodeForBTNode(child, template, graphName, includeNodeName));
            }

            return template.AddVariableDeclarationLine(compositeNode.GetType(), nodeData.name, nodeData,
                $"{graphName}.CreateComposite<{compositeNode.TypeName()}>({args.Join()})");
        }


        #endregion

        #region ------------------------------------------- FSMs ------------------------------------------

        static string GenerateCodeForState(NodeData data, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            string stateName = null;
            switch (data.node)
            {
                case Framework.Adaptations.State state:
                    var actionCode = GenerateActionCode(state.ActionReference, template);
                    stateName = template.AddVariableDeclarationLine(typeof(State), data.name, data,
                        $"{graphName}.CreateState({actionCode})");
                    break;
                case ProbabilisticState:
                    break;
            }
            return stateName;
        }

        static string GenerateCodeForTransition(NodeData data, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            switch (data.node)
            {
                case StateTransition transition: return GenerateStateTransitionCode(data, transition, template, graphName, includeNodeName);
                case ExitTransition exit: return GenerateExitTransitionCode(data, exit, template, graphName, includeNodeName);
                case PushTransition pushTransition: return GeneratePushTransitionCode(data, pushTransition, template, graphName, includeNodeName);
                case PopTransition popTransition: return GeneratePopTransitionCode(data, popTransition, template, graphName, includeNodeName);
                default: return null;
            }
        }

        static string GeneratePopTransitionCode(NodeData data, PopTransition popTransition, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();

            var parent = template.FindNode(data.parentIds.FirstOrDefault());
            string parentName = parent != null ? template.FindVariableName(parent) ??
                GenerateCodeForState(parent, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(parentName);

            var actionCode = GenerateActionCode(popTransition.ActionReference, template);
            if (actionCode != null) args.Add("action: " + actionCode);

            var perceptionCode = GeneratePerceptionCode(popTransition.PerceptionReference, template);
            if (perceptionCode != null) args.Add("perception: " + perceptionCode);

            if (popTransition.StatusFlags != StatusFlags.Active) args.Add("statusFlags: " + popTransition.StatusFlags.ToCodeFormat());

            return template.AddVariableDeclarationLine(popTransition.GetType(), data.name, data,
                $"{graphName}.CreatePopTransition({args.Join()})");

        }

        static string GeneratePushTransitionCode(NodeData data, PushTransition pushTransition, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();

            var child = template.FindNode(data.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForBTNode(child, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(childName);

            var parent = template.FindNode(data.parentIds.FirstOrDefault());
            string parentName = parent != null ? template.FindVariableName(parent) ??
                GenerateCodeForState(parent, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(parentName);

            var actionCode = GenerateActionCode(pushTransition.ActionReference, template);
            if (actionCode != null) args.Add("action: " + actionCode);

            var perceptionCode = GeneratePerceptionCode(pushTransition.PerceptionReference, template);
            if (perceptionCode != null) args.Add("perception: " + perceptionCode);
            if (pushTransition.StatusFlags != StatusFlags.Active) args.Add("statusFlags: " + pushTransition.StatusFlags.ToCodeFormat());

            return template.AddVariableDeclarationLine(pushTransition.GetType(), data.name, data,
                $"{graphName}.CreatePushTransition({args.Join()})");
        }

        private static string GenerateExitTransitionCode(NodeData data, ExitTransition exit, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();

            var parent = template.FindNode(data.parentIds.FirstOrDefault());
            string parentName = parent != null ? template.FindVariableName(parent) ??
                GenerateCodeForState(parent, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(parentName);

            args.Add(exit.ExitStatus.ToCodeFormat());

            var actionCode = GenerateActionCode(exit.ActionReference, template);
            if (actionCode != null) args.Add("action: " + actionCode);

            var perceptionCode = GeneratePerceptionCode(exit.PerceptionReference, template);
            if (perceptionCode != null) args.Add("perception: " + perceptionCode);

            if (exit.StatusFlags != StatusFlags.Active) args.Add("statusFlags: " + exit.StatusFlags.ToCodeFormat());

            return template.AddVariableDeclarationLine(exit.GetType(), data.name, data,
                $"{graphName}.CreateExitTransition({args.Join()})");
        }

        private static string GenerateStateTransitionCode(NodeData data, StateTransition stateTransition, ScriptTemplate template, string graphName, bool includeNodeName)
        {
            List<string> args = new List<string>();

            var child = template.FindNode(data.childIds.FirstOrDefault());
            string childName = child != null ? template.FindVariableName(child) ??
                GenerateCodeForBTNode(child, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(childName);

            var parent = template.FindNode(data.parentIds.FirstOrDefault());
            string parentName = parent != null ? template.FindVariableName(parent) ??
                GenerateCodeForState(parent, template, graphName, includeNodeName) : k_CodeForMissingNode;
            args.Add(parentName);

            var actionCode = GenerateActionCode(stateTransition.ActionReference, template);
            if (actionCode != null) args.Add("action: " + actionCode);

            var perceptionCode = GeneratePerceptionCode(stateTransition.PerceptionReference, template);
            if (perceptionCode != null) args.Add("perception: " + perceptionCode);
            if (stateTransition.StatusFlags != StatusFlags.Active) args.Add("statusFlags: " + stateTransition.StatusFlags.ToCodeFormat());

            return template.AddVariableDeclarationLine(stateTransition.GetType(), data.name, data,
                $"{graphName}.CreateTransition({args.Join()})");
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
        /// Generates code for properties initialized with setter methods.
        /// .SetVARNAME(VARVALUE).Set...
        /// </summary>
        static void GenerateNodePropertiesCode(Node node, string nodeVariableName, ScriptTemplate template)
        {
            var type = node.GetType();
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.FieldType.IsAssignableFrom(typeof(Action)))
                {
                    var value = (Action)field.GetValue(node);
                    if (value != null)
                    {
                        var actionCode = GenerateActionCode((Action)field.GetValue(node), template, true);
                        template.AddLine($"{nodeVariableName}.Action = {actionCode};");
                    }
                }
                else if (field.FieldType.IsAssignableFrom(typeof(Perception)))
                {
                    var value = (Perception)field.GetValue(node);
                    if (value != null)
                    {
                        var perceptionCode = GeneratePerceptionCode((Perception)field.GetValue(node), template, true);
                        template.AddLine($"{nodeVariableName}.Perception = {perceptionCode};");
                    }
                }
                else
                {
                    template.AddvariableReasignation(field.FieldType, $"{nodeVariableName}.{field.Name}",
                    $"{nodeVariableName}_{field.Name}", field.GetValue(node));
                }
            }
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

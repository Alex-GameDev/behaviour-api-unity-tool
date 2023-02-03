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

namespace BehaviourAPI.Unity.Editor
{
    public static class ScriptGeneration
    {
        private static readonly string[] basicNamespaces =
        {
            "UnityEngine",
            "System.Collections",
            "System.Collections.Generic",
            "BehaviourAPI.Unity.Runtime",
            "BehaviourAPI.Core",
            "BehaviourAPI.Core.Actions",
            "BehaviourAPI.Core.Perceptions"
        };

        private static readonly string[] bannedNamespaces =
        {
            "BehaviourAPI.Unity.Framework.Adaptations"
        };

        private static readonly string k_CodeForMissingNode = "null /*Missing node*/";

        public static void GenerateScript(string path, string name, BehaviourSystemAsset asset)
        {
            //var scriptPath = EditorUtility.SaveFilePanel("Select a folder to save the script", path, $"{name}.cs", "CS");
            var scriptPath = $"{path}{name}.cs";
            if(!string.IsNullOrEmpty(scriptPath))
            {
                Object obj = CreateScript(scriptPath, asset);
                AssetDatabase.Refresh();
                ProjectWindowUtil.ShowCreatedAsset(obj);
            }
        }

        static Object CreateScript(string path, BehaviourSystemAsset asset)
        {
            string folderPath = path.Substring(0, path.LastIndexOf("/") + 1);
            string scriptName = path.Substring(path.LastIndexOf("/") + 1).Replace(".cs", "");

            //ScriptTemplate scriptTemplate = new ScriptTemplate(scriptName, nameof(CodeBehaviourRunner));

            //foreach(var ns in basicNamespaces)
            //{
            //    scriptTemplate.AddUsingDirective(ns);
            //}

            //// Add main using directives
            //scriptTemplate.AddUsingDirective("UnityEngine");
            //scriptTemplate.AddUsingDirective("System.Collections.Generic");
            //scriptTemplate.AddUsingDirective("BehaviourAPI.Core");
            //scriptTemplate.AddUsingDirective("BehaviourAPI.Unity.Runtime");
            //scriptTemplate.AddUsingDirective("BehaviourAPI.Core.Actions");
            //scriptTemplate.AddUsingDirective("BehaviourAPI.Core.Perceptions");
            //scriptTemplate.AddUsingDirective("BehaviourAPI.Unity.Runtime.Extensions");

            //scriptTemplate.OpenMethodDeclaration("CreateGraph", nameof(BehaviourGraph), "protected override", $"HashSet<{typeof(BehaviourGraph).Name}> registeredGraphs");

            //// Add the class

            //var rootName = "";

            //Dictionary<Type, GraphAdapter> graphAdapterMap = new Dictionary<Type, GraphAdapter>();

            //for(int i = 0; i < asset.Graphs.Count; i++)
            //{
            //    var graph = asset.Graphs[i].Graph;

            //    if(graph == null)
            //    {
            //        Debug.LogWarning($"Graph {i} is empty.");
            //        scriptTemplate.AddLine($"//Graph {i} is empty.");
            //        continue;
            //    }

            //    if(!graphAdapterMap.TryGetValue(graph.GetType(), out GraphAdapter adapter))
            //    {
            //        adapter = GraphAdapter.FindAdapter(graph);
            //        graphAdapterMap.Add(graph.GetType(), adapter);
            //    }

            //    var graphName = !string.IsNullOrEmpty(asset.Graphs[i].Name) ? asset.Graphs[i].Name : graph.TypeName().ToLower();
            //    graphName = adapter.CreateGraphLine(asset.Graphs[i], scriptTemplate, graphName);
            //    scriptTemplate.AddLine($"registeredGraphs.Add({graphName});");

            //    if (i == 0)
            //    {
            //        rootName = graphName;
            //    }
            //}

            //for (int i = 0; i < asset.Graphs.Count; i++)
            //{
            //    var graph = asset.Graphs[i].Graph;
            //    var adapter = graphAdapterMap[graph.GetType()];
            //    scriptTemplate.AddLine("");
            //    adapter.ConvertAssetToCode(asset.Graphs[i], scriptTemplate);

            //}

            //if (!string.IsNullOrEmpty(rootName))
            //{
            //    scriptTemplate.AddLine("");
            //    scriptTemplate.AddLine($"return {rootName};");
            //}

            //scriptTemplate.CloseMethodDeclaration();

            //var content = scriptTemplate.ToString();

            var content = GenerateCode(asset, scriptName);

            UTF8Encoding encoding = new UTF8Encoding(true, false);
            StreamWriter writer = new StreamWriter(Path.GetFullPath(path), false, encoding);
            writer.Write(content);
            writer.Close();

            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath(path, typeof(Object));
        }

        static string GenerateCode(BehaviourSystemAsset asset, string scriptName)
        {
            ScriptTemplate scriptTemplate = new ScriptTemplate(scriptName, nameof(CodeBehaviourRunner));

            foreach (var ns in basicNamespaces)
            {
                scriptTemplate.AddUsingDirective(ns);
            }

            scriptTemplate.OpenMethodDeclaration("CreateGraph", nameof(BehaviourGraph), 
                "protected override", parameters: $"HashSet<{typeof(BehaviourGraph).Name}> registeredGraphs");


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

            scriptTemplate.AddLine("");

            foreach (var graphAsset in asset.Graphs)
            {
                GenerateCodeForGraph(graphAsset, scriptTemplate);
                scriptTemplate.AddLine("");
            }

            if(asset.Graphs.Count > 0)
            {
                var mainGraphName = scriptTemplate.FindVariableName(asset.Graphs[0]);
                scriptTemplate.AddLine("return " + (mainGraphName ?? "null") + ";");
            }
            else
            {
                scriptTemplate.AddLine("return null;");
            }

            foreach (var ns in bannedNamespaces)
            {
                scriptTemplate.RemoveUsingDirective(ns);
            }


            scriptTemplate.CloseMethodDeclaration();

            return scriptTemplate.ToString();
        }

        static string AddCreateGraphLine(GraphAsset graphAsset, string graphName, ScriptTemplate template)
        {
            var graph = graphAsset.Graph;
            var type = graph.GetType();

            if (graph is UtilitySystem us)
            {
                graphName = template.AddVariableInstantiationLine(type, graphName, graphAsset, us.Inertia, us.UtilityThreshold);
            }
            else
            {
                graphName = template.AddVariableInstantiationLine(type, graphName, graphAsset);
            }
            return graphName;
        }

        static void GenerateCodeForGraph(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            var graph = graphAsset.Graph;
            if (graph is FSM fsm)
            {
                var states = graphAsset.Nodes.FindAll(n => n.Node is State);
                var transitions = graphAsset.Nodes.FindAll(n => n.Node is Transition);

                foreach (var state in states)
                {
                    GenerateCodeForState(state, scriptTemplate, graphName);
                }
                scriptTemplate.AddLine("");

                foreach (var transition in transitions)
                {
                    GenerateCodeForTransition(transition, scriptTemplate, graphName);
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
                        GenerateCodeForBTNode(btNodeAsset, scriptTemplate, graphName);
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
                        GenerateCodeForFactor(factorAsset, scriptTemplate, graphName);
                    }
                }

                scriptTemplate.AddLine("");

                foreach(var selectableAsset in selectables)
                {
                    if (scriptTemplate.FindVariableName(selectableAsset) == null)
                    {
                        GenerateCodeForSelectableNode(selectableAsset, scriptTemplate, graphName);
                    }
                }

                if (selectables.Count > 0)
                {
                    var defaultElement = scriptTemplate.FindVariableName(selectables[0]);
                    if (defaultElement != null)
                    {
                        scriptTemplate.AddLine($"{graphName}.SetDefaultUtilityElement({defaultElement});");
                    }
                }
            }
        }

        /// <summary>
        /// Generates code for a utility system's factor. If the factor has childs, the method generates code recursively.
        /// FACTORTYPE VARNAME = USNAME.CreateFACTORTYPE(args).SetVARNAME(VARVALUE)...;
        /// </summary>
        static string GenerateCodeForFactor(NodeAsset asset, ScriptTemplate template, string graphName)
        {
            var factor = asset.Node as Factor;

            if(factor == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : factor.TypeName().ToLower();
            string typeName = factor.TypeName();

            var methodCode = "";

            // If is a variable factor, generates code for the serialized method.
            if (factor is VariableFactor variableFactor)
            {
                var functionCode = GenerateSerializedMethodCode(variableFactor.variableFunction, template) ?? "() => 0f /*Missing function*/";
                methodCode = $"CreateVariableFactor({functionCode}, {variableFactor.min.ToCodeFormat()}, {variableFactor.max.ToCodeFormat()})";
            }
            // If is a function factor, generates also code for the child if wasn't generated yet. The generates code for the setters.
            else if (factor is FunctionFactor functionFactor)
            {
                var child = asset.Childs.FirstOrDefault();
                string childName = child != null ? template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName) : k_CodeForMissingNode;
                string setterCode = GenerateSetterCode(functionFactor, template);
                methodCode = $"CreateFunctionFactor<{typeName}>({childName}){setterCode}";
            }
            // If is a fusion factor, generates also code for all its children if wasn't generated yet. Also generates code for the weights if necessary.
            else if (factor is FusionFactor fusionFactor)
            {
                var args = new List<string>();
                foreach (var child in asset.Childs)
                {
                    var childName = template.FindVariableName(child) ?? GenerateCodeForFactor(child, template, graphName);
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
        static string GenerateCodeForSelectableNode(NodeAsset asset, ScriptTemplate template, string graphName)
        {
            UtilitySelectableNode selectableNode = asset.Node as UtilitySelectableNode;

            if (selectableNode == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : selectableNode.TypeName().ToLower();
            string typeName = selectableNode.TypeName();

            var method = "";

            var args = new List<string>();

            // If is an utility action, generates code for the child factor and for the action if necessary.
            if (selectableNode is UtilityAction action)
            {
                args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);

                if (action.Action != null) args.Add(GenerateActionCode(action.Action, template));
                if (action.FinishSystemOnComplete) args.Add("finishOnComplete: true");
                if (selectableNode.IsRoot) args.Add("root: true");
                method = $"CreateUtilityAction({args.Join()})";
            }
            // If is an utility exit node, generates code for the child factor and the exit status.
            else if (selectableNode is UtilityExitNode exitNode)
            {
                args.Add(template.FindVariableName(asset.Childs.FirstOrDefault()) ?? k_CodeForMissingNode);
                args.Add(exitNode.ExitStatus.ToCodeFormat());
                if (selectableNode.IsRoot) args.Add("root: true");
                method = $"CreateUtilityExitNode({args.Join()})";
            }
            // Us is an utility bucket, generates code for the properties and all the childs.
            else if (selectableNode is UtilityBucket bucket)
            {
                args.Add($"{bucket.IsRoot.ToCodeFormat()}");
                args.Add($"{bucket.UtilityThreshold.ToCodeFormat()}");
                args.Add($"{bucket.Inertia.ToCodeFormat()}");
                args.Add($"{bucket.BucketThreshold.ToCodeFormat()}");

                asset.Childs.ForEach(child =>
                {
                    string childName = template.FindVariableName(child) ?? GenerateCodeForSelectableNode(child, template, graphName);
                    if (childName != null) args.Add(childName);
                });
                method = $"CreateUtilityBucket({args.Join()})";
            }
            return template.AddVariableDeclarationLine(selectableNode.GetType(), nodeName, asset, $"{graphName}.{method}");
        }

        static string GenerateCodeForBTNode(NodeAsset asset, ScriptTemplate template, string graphName)
        {
            var btNode = asset.Node as BTNode;

            if(btNode == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = asset.Name ?? btNode.TypeName().ToLower();
            var typeName = btNode.TypeName();

            var method = "";

            if (btNode is CompositeNode composite)
            {
                var args = new List<string>();
                args.Add(composite.IsRandomized.ToCodeFormat());

                foreach (var child in asset.Childs)
                {
                    var childName = GenerateCodeForBTNode(child, template, graphName);
                    if (childName != null) args.Add(childName);
                }
                method = $"CreateComposite<{typeName}>({args.Join()})";
            }
            else if (btNode is DecoratorNode decorator)
            {
                var childName = GenerateCodeForBTNode(asset.Childs.FirstOrDefault(), template, graphName) ?? "null /* Missing child */";

                var propertyCode = GenerateSetterCode(decorator, template);
                method = $"CreateDecorator<{typeName}>({childName}){propertyCode}";
            }
            else if (btNode is LeafNode leaf)
            {
                var actionCode = GenerateActionCode(leaf.Action, template) ?? "null /* Missing action */";
                method = $"CreateLeafNode({actionCode})";
            }

            return template.AddVariableDeclarationLine(btNode.GetType(), nodeName, asset, $"{graphName}.{method}");
        }


        static string GenerateCodeForState(NodeAsset asset, ScriptTemplate template, string graphName)
        {
            var state = asset.Node as State;

            if (state == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = !string.IsNullOrEmpty(asset.Name) ? asset.Name : state.TypeName().ToLower();
            var typeName = state.TypeName();
            var actionCode = GenerateActionCode(state.Action, template);
            return template.AddVariableDeclarationLine(state.GetType(), nodeName, asset, $"{graphName}.CreateState({actionCode})");
        }

        static string GenerateCodeForTransition(NodeAsset asset, ScriptTemplate template, string graphName)
        {
            var transition = asset.Node as Transition;

            if (transition == null)
            {
                return k_CodeForMissingNode;
            }

            var nodeName = asset.Name ?? transition.TypeName().ToLower();
            var method = "";

            var args = new List<string>();
            var sourceState = template.FindVariableName(asset.Parents.FirstOrDefault()) ?? "null/*ERROR*/";
            args.Add(sourceState);

            if(transition is StateTransition stateTransition)
            {
                var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                args.Add(targetState);

                var perceptionCode = "";
                if(stateTransition.Perception != null)
                {
                    perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                }
                else if (stateTransition is Framework.Adaptations.StateTransition adaptedTransition && 
                    adaptedTransition.StatusFlags != StatusFlags.None)
                {
                    perceptionCode = $"new {nameof(ExecutionStatusPerception)}({sourceState}, {adaptedTransition.StatusFlags.ToCodeFormat()})";
                }
                if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);
               
                method = "CreateTransition";
            }
            else if (transition is ExitTransition exitTransition)
            {
                args.Add(exitTransition.ExitStatus.ToCodeFormat());

                var perceptionCode = "";
                if (exitTransition.Perception != null)
                {
                    perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                }
                else if (exitTransition is Framework.Adaptations.ExitTransition adaptedTransition &&
                    adaptedTransition.StatusFlags != StatusFlags.None)
                {
                    perceptionCode = $"new {nameof(ExecutionStatusPerception)}({sourceState}, {adaptedTransition.StatusFlags.ToCodeFormat()})";
                }
                if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);

                method = "CreateExitTransition";
            }
            else if (transition is PopTransition popTransition)
            {
                var perceptionCode = "";
                if (popTransition.Perception != null)
                {
                    perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                }
                else if (popTransition is Framework.Adaptations.PopTransition adaptedTransition &&
                    adaptedTransition.StatusFlags != StatusFlags.None)
                {
                    perceptionCode = $"new {nameof(ExecutionStatusPerception)}({sourceState}, {adaptedTransition.StatusFlags.ToCodeFormat()})";
                }
                if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);

                method = "CreatePopTransition";
            }
            else if (transition is PushTransition pushTransition)
            {
                var targetState = template.FindVariableName(asset.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                args.Add(targetState);

                var perceptionCode = "";
                if (pushTransition.Perception != null)
                {
                    perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                }
                else if (pushTransition is Framework.Adaptations.PushTransition adaptedTransition &&
                    adaptedTransition.StatusFlags != StatusFlags.None)
                {
                    perceptionCode = $"new {nameof(ExecutionStatusPerception)}({sourceState}, {adaptedTransition.StatusFlags.ToCodeFormat()})";
                }
                if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);

                method = "CreatePopTransition";
            }

            if (transition.Action != null)
            {
                var actionCode = GenerateActionCode(transition.Action, template);
                if (!string.IsNullOrEmpty(actionCode)) args.Add(actionCode);
            }

            if (!transition.isPulled) args.Add("isPulled: false");

            return template.AddVariableDeclarationLine(transition.GetType(), nodeName, asset, $"{graphName}.{method}({args.Join()})");
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
                // Add arguments
                return $"new {unityAction.TypeName()}( )";
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
        /// Generates code for a perception inline.
        /// new PERCEPTIONTYPE(args)
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
                // Add arguments
                return $"new {unityPerception.TypeName()}()";
            }
            else
                return null;
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
                            argCode = GeneratePerceptionCode(field.GetValue(node) as Perception, scriptTemplate);
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
    }
}

using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using ExitTransition = BehaviourAPI.StateMachines.ExitTransition;
using State = BehaviourAPI.StateMachines.State;
using Transition = BehaviourAPI.StateMachines.Transition;

namespace BehaviourAPI.Unity.Editor
{
    [CustomAdapter(typeof(FSM))]
    public class StateMachineAdapter : GraphAdapter
    {
        #region --------------- Assets generation ---------------

        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ---------------- Code generation ----------------

        public override void ConvertAssetToCode(GraphAsset graphAsset, ScriptTemplate scriptTemplate)
        {
            var graphName = scriptTemplate.FindVariableName(graphAsset);

            var states = graphAsset.Nodes.FindAll(n => n.Node is State);
            var transitions = graphAsset.Nodes.FindAll(n => n.Node is Transition);

            foreach(var state in states)
            {
                AddState(state, scriptTemplate, graphName);
            }

            scriptTemplate.AddLine("");

            foreach(var transition in transitions)
            {
                AddTransition(transition, scriptTemplate, graphName);
            }

            var entryState = states.FirstOrDefault();

            if (entryState != null)
            {
                var entryStateName = scriptTemplate.FindVariableName(entryState);
                if (!string.IsNullOrEmpty(entryStateName)) scriptTemplate.AddLine($"{graphName}.SetEntryState({entryStateName});");
            }
        }


        public override string CreateGraphLine(GraphAsset graphAsset, ScriptTemplate scriptTemplate, string graphName)
        {
            if (graphAsset.Graph is FSM fsm)
            {
                scriptTemplate.AddUsingDirective(typeof(FSM).Namespace);
                return scriptTemplate.AddVariableInstantiationLine(fsm.TypeName(), graphName, graphAsset);
            }
            else
            {
                return null;
            }
        }


        void AddState(NodeAsset node, ScriptTemplate template, string graphName)
        {           
            if(node.Node is State state)
            {
                var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : state.TypeName().ToLower();
                string typeName = state.TypeName();
                var actionCode = GenerateActionCode(state.Action, template);
                template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.CreateState({actionCode})");
            }
        }

        void AddTransition(NodeAsset node, ScriptTemplate template, string graphName)
        {
            if (node.Node is Transition transition)
            {
                var nodeName = !string.IsNullOrEmpty(node.Name) ? node.Name : transition.TypeName().ToLower();
                string typeName = transition.TypeName();

                var args = new List<string>();

                var sourceState = template.FindVariableName(node.Parents.FirstOrDefault()) ?? "null/*ERROR*/";
                args.Add(sourceState);

                var methodName = string.Empty;
                if (transition is StateTransition stateTransition)
                {
                    var targetState = template.FindVariableName(node.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                    args.Add(targetState);
                    if(stateTransition is FinishExecutionTransition finish)
                    {
                        args.Add($"new {nameof(ExecutionStatusPerception)}({sourceState}, {finish._statusFlags.ToCodeFormat()})");
                    }
                    else
                    {
                        if (transition.Perception != null)
                        {
                            var perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                            if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);
                        }
                    }
                    methodName = "CreateTransition";
                }
                else if(transition is ExitTransition exitTransition)
                {
                    args.Add(exitTransition.ExitStatus.ToCodeFormat());

                    if (transition.Perception != null)
                    {
                        var perceptionCode = GeneratePerceptionCode(transition.Perception, template);
                        if (!string.IsNullOrEmpty(perceptionCode)) args.Add(perceptionCode);
                    }
                    methodName = "CreateExitTransition";
                }

                if (transition.Action != null)
                {
                    var actionCode = GenerateActionCode(transition.Action, template);
                    if (!string.IsNullOrEmpty(actionCode)) args.Add(actionCode);
                }

                if (!transition.isPulled) args.Add("isPulled: false");

                template.AddVariableDeclarationLine(typeName, nodeName, node, $"{graphName}.{methodName}({args.Join()})");
            }
        }

        #endregion

        #region ------------------- Rendering -------------------

        NodeView _entryStateView;
        protected override List<Type> MainTypes => new List<Type>
        {
            typeof(State),
            typeof(Transition)
        };

        protected override List<Type> ExcludedTypes => new List<Type> { 
            typeof(State), 
            typeof(ExitTransition), 
            typeof(StateTransition), 
            typeof(ProbabilisticState) 
        };

        protected override void DrawGraphDetails(GraphAsset graphAsset, BehaviourGraphView graphView, List<NodeView> nodeViews)
        {
            var firstState = graphAsset.Nodes.FirstOrDefault(n => n.Node is State);
            if (firstState != null) ChangeEntryState(nodeViews.Find(n => n.Node == firstState));
        }

        protected override NodeView.Layout NodeLayout => NodeView.Layout.Cyclic;

        protected override void SetUpNodeContextMenu(NodeView node, ContextualMenuPopulateEvent menuEvt)
        {
            menuEvt.menu.AppendAction("Set entry state", _ => ChangeEntryState(node), 
                _ => (node.Node != null && node.Node.Node is State) ? (node == _entryStateView).ToMenuStatus() : DropdownMenuAction.Status.Hidden);
        }

        protected override void SetUpPortsAndDetails(NodeView nodeView)
        {
            if (nodeView.Node.Node.MaxInputConnections != 0)
            {
                var port1 = CreatePort(nodeView, Direction.Input, PortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.left = new StyleLength(new Length(50, LengthUnit.Percent));

                var port2 = CreatePort(nodeView, Direction.Input, PortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.top = new StyleLength(new Length(50, LengthUnit.Percent));

                var port3 = CreatePort(nodeView, Direction.Input, PortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.right = new StyleLength(new Length(50, LengthUnit.Percent));

                var port4 = CreatePort(nodeView, Direction.Input, PortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));
            }
            else
            {
                nodeView.inputContainer.style.display = DisplayStyle.None;
            }

            if (nodeView.Node.Node.MaxOutputConnections != 0)
            {
                var port1 = CreatePort(nodeView, Direction.Output, PortOrientation.Bottom);
                port1.style.position = Position.Absolute;
                port1.style.top = 0; port1.style.right = new StyleLength(new Length(50, LengthUnit.Percent));

                var port2 = CreatePort(nodeView, Direction.Output, PortOrientation.Right);
                port2.style.position = Position.Absolute;
                port2.style.right = 0; port2.style.bottom = new StyleLength(new Length(50, LengthUnit.Percent));

                var port3 = CreatePort(nodeView, Direction.Output, PortOrientation.Top);
                port3.style.position = Position.Absolute;
                port3.style.bottom = 0; port3.style.left = new StyleLength(new Length(50, LengthUnit.Percent));

                var port4 = CreatePort(nodeView, Direction.Output, PortOrientation.Left);
                port4.style.position = Position.Absolute;
                port4.style.left = 0; port4.style.top = new StyleLength(new Length(50, LengthUnit.Percent));
            }
            else
                nodeView.outputContainer.style.display = DisplayStyle.None;
        }

        protected override GraphViewChange ViewChanged(BehaviourGraphView graphView, GraphViewChange change)
        {
            var rootNode = graphView.GraphAsset.Nodes.FirstOrDefault(n => n.Node is State);

            if (rootNode != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(rootNode);
                var view = graphView.nodes.Select(n => n as NodeView).ToList().Find(n => n.Node == rootNode);
                ChangeEntryState(view);
            }
            return change;
        }

        void ChangeEntryState(NodeView newStartNode)
        {
            if (newStartNode == null || newStartNode.Node.Node is not State) return;

            var graphView = newStartNode.GraphView;

            if (_entryStateView != null)
            {
                _entryStateView.RootElement.Disable();
            }

            _entryStateView = newStartNode;
            if (_entryStateView != null)
            {
                graphView.GraphAsset.Nodes.MoveAtFirst(_entryStateView.Node);
                _entryStateView.RootElement.Enable();
            }
        }

        protected override void SetUpGraphContextMenu(BehaviourGraphView graph, ContextualMenuPopulateEvent menuEvt)
        {
        }

        #endregion
    }
}

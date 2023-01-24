using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.StateMachines;
using BehaviourAPI.UtilitySystems;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using ExitTransition = BehaviourAPI.StateMachines.ExitTransition;
using State = BehaviourAPI.StateMachines.State;
using Transition = BehaviourAPI.StateMachines.Transition;

namespace BehaviourAPI.Unity.Editor
{
    [CustomConverter(typeof(FSM))]
    public class StateMachineConverter : GraphConverter
    {
        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(FSM)) return null;

            return GraphAsset.Create("new graph", typeof(FSM));
        }

        public override string AddCreateGraphLine(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            scriptTemplate.AddUsingDirective(typeof(FSM).Namespace);
            return base.AddCreateGraphLine(asset, scriptTemplate);
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph is not FSM fsm) return;

            graphName = scriptTemplate.FindVariableName(asset);

            scriptTemplate.AddLine($"// FSM - {graphName}:");

            var states = asset.Nodes.FindAll(n => n.Node is State);
            var transitions = asset.Nodes.FindAll(n => n.Node is Transition);

            states.ForEach(stateNode =>
            {
                var state = stateNode.Node as State;
                scriptTemplate.AddVariableDeclarationLine(nameof(State), stateNode.Name, stateNode, $"{graphName}.CreateState({GetActionCode(state.Action, scriptTemplate)})");
            });

            transitions.ForEach(trNode =>
            {
                var transition = trNode.Node as Transition;
                var nodeName = string.IsNullOrEmpty(trNode.name) ? transition.TypeName().ToLower() : trNode.name;

                var arguments = new List<string>();

                var sourceState = scriptTemplate.FindVariableName(trNode.Parents.FirstOrDefault()) ?? "null/*ERROR*/";

                arguments.Add(sourceState);

                var methodName = string.Empty;

                if(transition is StateTransition stateTransition)
                {                    
                    var targetState = scriptTemplate.FindVariableName(trNode.Childs.FirstOrDefault()) ?? "null/*ERROR*/";
                    arguments.Add(targetState);

                    if(stateTransition is FinishExecutionTransition finish)
                    {
                        arguments.Add($"new {nameof(ExecutionStatusPerception)}({sourceState}, {finish._statusFlags.ToCodeFormat()})");
                    }
                    else
                    {
                        if (transition.Perception != null) arguments.Add(GetPerceptionCode(transition.Perception, scriptTemplate));
                    }                    

                    methodName = "CreateTransition";
                }
                else if(transition is ExitTransition exitTransition)
                {
                    arguments.Add(exitTransition.ExitStatus.ToCodeFormat());
                    if (transition.Perception != null) arguments.Add(GetPerceptionCode(transition.Perception, scriptTemplate));
                    methodName = "CreateExitTransition";
                }

                if (transition.Action != null) arguments.Add("action: " + GetActionCode(transition.Action, scriptTemplate));
                if (!transition.isPulled) arguments.Add("isPulled: false");

                if (!string.IsNullOrEmpty(methodName))
                {
                    scriptTemplate.AddVariableDeclarationLine(nameof(Transition), nodeName, trNode,
                        $"{graphName}.{methodName}({string.Join(", ", arguments)})");
                }
            });

            var entryState = states.FirstOrDefault();

            if (entryState != null)
            {
                var entryStateName = scriptTemplate.FindVariableName(entryState);
                if(!string.IsNullOrEmpty(entryStateName)) scriptTemplate.AddLine($"{graphName}.SetEntryState({entryStateName});");
            }           
        }
    }
}

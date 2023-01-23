using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using State = BehaviourAPI.StateMachines.State;

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

            var states = new Dictionary<State, string>();
            var internalTransitions = new Dictionary<NodeAsset, string>();
            var exitTransitions = new Dictionary<NodeAsset, string>();

            //asset.Nodes.ForEach(node =>
            //{
            //    var fsmNode = node.Node as FSMNode;

            //    if(fsmNode is State state)
            //    {
            //        scriptTemplate.AddVariableDeclarationLine(node.Name);
            //        scriptTemplate.BeginMethod($"{graphName}.CreateState");
            //        AddAction(state.Action, scriptTemplate);
            //        scriptTemplate.CloseMethodOrVariableAsignation();

            //        states.Add(state, node.Name);
            //    }
            //    else if(fsmNode is StateTransition stateTransition)
            //    {
            //        internalTransitions.Add(stateTransition, node.Name);
            //    }
            //    else if(fsmNode is ExitTransition exitTransition)
            //    {
            //        exitTransitions.Add(exitTransition, node.Name);
            //    }
            //});

            //scriptTemplate.AddEmptyLine();

            //foreach(KeyValuePair<NodeAsset, string> tr in internalTransitions)
            //{
            //    var stateFromName = scriptTemplate.FindVariableName(tr.Key.Childs[0]);
            //    var stateToName = scriptTemplate.FindVariableName(tr.Key.Parents[0]);

            //    var transitionName = scriptTemplate.AddVariableDeclarationLine(nameof(Transition), tr.Value);

            //    scriptTemplate.AddVariableDeclarationLine(tr.Value);
            //    scriptTemplate.BeginMethod($"{graphName}.CreateTransition");
            //    //scriptTemplate.AddParameter();
            //}
        }
    }
}

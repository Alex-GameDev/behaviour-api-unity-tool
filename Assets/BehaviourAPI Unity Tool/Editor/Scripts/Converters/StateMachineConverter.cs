using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;
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

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(FSM)) return;

            var graph = asset.Graph;
            scriptTemplate.AddVariableDeclaration(nameof(FSM), asset.Name);
            scriptTemplate.AddEmptyLine();           
        }
    }
}

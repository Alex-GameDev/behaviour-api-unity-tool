using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.StateMachines;
using BehaviourAPI.Unity.Runtime;

namespace BehaviourAPI.Unity.Editor
{
    [CustomConverter(typeof(BehaviourTree))]
    public class BehaviourTreeConverter : GraphConverter
    {
        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(BehaviourTree)) return null;

            return GraphAsset.Create("new graph", typeof(BehaviourTree));
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(BehaviourTree)) return;

            var graph = asset.Graph;
            scriptTemplate.AddVariableDeclaration(nameof(BehaviourTree), asset.Name);
            scriptTemplate.AddEmptyLine();
        }
    }
}

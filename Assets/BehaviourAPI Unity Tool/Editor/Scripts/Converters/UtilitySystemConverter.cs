using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UtilitySystems;

namespace BehaviourAPI.Unity.Editor
{
    [CustomConverter(typeof(UtilitySystem))]
    public class UtilitySystemConverter : GraphConverter
    {
        public override GraphAsset ConvertCodeToAsset(BehaviourGraph graph)
        {
            if (graph.GetType() != typeof(UtilitySystem)) return null;

            return GraphAsset.Create("new graph", typeof(UtilitySystem));
        }

        public override void ConvertAssetToCode(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            if (asset.Graph.GetType() != typeof(UtilitySystem)) return;

            var graph = asset.Graph;
        }

        public override string AddCreateGraphLine(GraphAsset asset, ScriptTemplate scriptTemplate)
        {
            scriptTemplate.AddUsingDirective(typeof(UtilitySystem).Namespace);

            var utilitySystem = asset.Graph as UtilitySystem;
            return scriptTemplate.AddVariableInstantiationLine(asset.Graph.TypeName(), asset.Name, asset, utilitySystem.Inertia.ToCodeFormat(), utilitySystem.UtilityThreshold.ToCodeFormat());
        }
    }
}

using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.UtilitySystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            scriptTemplate.AddVariableDeclaration(nameof(UtilitySystem), asset.Name);
            scriptTemplate.AddEmptyLine();
        }
    }
}

using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public class AssetSubgraphAction : SubsystemAction, IBuildable
    {
        [SerializeField] BehaviourSystem subgraph;

        public AssetSubgraphAction() : base(null)
        {
        }

        public void Build(BuildData data)
        {
            var runtimeData = subgraph.GetBehaviourSystemData();
            SubSystem = runtimeData.BuildSystem(data.Runner);
        }
    }
}

using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Action that stores its subgraph in an asset.
    /// </summary>
    public class SubgraphAction : SubsystemAction, IBuildable
    {
        [HideInInspector] public string subgraphId;

        public SubgraphAction() : base(null)
        {
        }

        public SubgraphAction(BehaviourEngine subSystem, bool executeOnLoop = false, bool dontStopOnInterrupt = false) : base(subSystem, executeOnLoop, dontStopOnInterrupt)
        {
        }

        public void Build(SystemData data)
        {
            SubSystem = data.graphs.Find(g => g.id == subgraphId).graph;
        }
    }
}

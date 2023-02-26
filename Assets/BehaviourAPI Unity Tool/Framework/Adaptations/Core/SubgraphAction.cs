using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Action that stores its subgraph in an asset.
    /// </summary>
    public class SubgraphAction : SubsystemAction
    {
        [HideInInspector][SerializeField] GraphAsset subgraph;

        public GraphAsset Subgraph { get => subgraph; set => subgraph = value; }

        public SubgraphAction() : base(null)
        {
        }

        public SubgraphAction(BehaviourEngine subSystem, bool executeOnLoop = false, bool dontStopOnInterrupt = false) : base(subSystem, executeOnLoop, dontStopOnInterrupt)
        {
        }

        public override void SetExecutionContext(ExecutionContext context)
        {
            SubSystem = Subgraph.Graph;
            base.SetExecutionContext(context);
        }
    }
}

using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework.Adaptations
{
    /// <summary>
    /// Action that stores its subgraph in an asset.
    /// </summary>
    public class SubgraphAction : Action
    {
        [HideInInspector][SerializeField] GraphAsset subgraph;
        public GraphAsset Subgraph { get => subgraph; set => subgraph = value; }

        public override void SetExecutionContext(ExecutionContext context)
        {
            subgraph.Graph.SetExecutionContext(context);
        }

        public override void Start()
        {
            subgraph.Graph.Start();
        }

        public override void Stop()
        {
            subgraph.Graph.Stop();
        }

        public override Status Update()
        {
            subgraph.Graph.Update();
            return subgraph.Graph.Status;
        }
    }
}

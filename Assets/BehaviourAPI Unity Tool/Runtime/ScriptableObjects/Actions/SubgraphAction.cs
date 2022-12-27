using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public class SubgraphAction : Action
    {
        [SerializeField] GraphAsset subgraph;

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

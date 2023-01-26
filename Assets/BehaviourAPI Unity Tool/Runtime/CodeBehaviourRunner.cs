using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class CodeBehaviourRunner : BehaviourRunner
    {
        BehaviourGraph rootGraph;

        HashSet<BehaviourGraph> allgraphs = new HashSet<BehaviourGraph>();

        protected abstract BehaviourGraph CreateGraph(HashSet<BehaviourGraph> registeredGraphs);
        protected override void OnAwake()
        {
            rootGraph = CreateGraph(allgraphs);
            Debug.Log(allgraphs.Count);
        }

        protected override void OnStart() => rootGraph.Start();
        protected override void OnUpdate() => rootGraph.Update();

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return BehaviourSystemAsset.CreateSystem(allgraphs);
        }
    }
}

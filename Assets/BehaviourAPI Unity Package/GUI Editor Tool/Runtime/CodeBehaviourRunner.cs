using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class CodeBehaviourRunner : BehaviourRunner
    {

        Dictionary<BehaviourGraph, string> allgraphs = new Dictionary<BehaviourGraph, string>();

        protected abstract BehaviourGraph CreateGraph();


        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return BehaviourSystemAsset.CreateSystem(allgraphs);
        }
        protected override BehaviourGraph GetExecutionGraph() => CreateGraph();

        public void RegisterGraph(BehaviourGraph graph, string name = "")
        {
            allgraphs.Add(graph, name);
        }
    }
}

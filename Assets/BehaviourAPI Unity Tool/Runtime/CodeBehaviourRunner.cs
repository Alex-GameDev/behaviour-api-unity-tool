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

        protected abstract BehaviourGraph CreateGraph();

        protected override void OnAwake()
        {
            rootGraph = CreateGraph();

            if (rootGraph != null) rootGraph.SetExecutionContext(new UnityExecutionContext(gameObject));
        }

        protected override void OnStart()
        {
            rootGraph.Start();
        }

        protected override void OnUpdate()
        {
            rootGraph.Update();
        }

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return BehaviourSystemAsset.CreateSystem(allgraphs);
        }

        public void RegisterGraph(BehaviourGraph graph)
        {
            allgraphs.Add(graph);
        }
    }
}

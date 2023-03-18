using BehaviourAPI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [Serializable]
    public class SystemData : ICloneable
    {
        [HideInInspector] public List<GraphData> graphs = new List<GraphData>();
        [HideInInspector] public List<PushPerceptionData> pushPerceptions = new List<PushPerceptionData>();

        public SystemData()
        {
        }

        //TODO: 
        public SystemData(Dictionary<BehaviourGraph, string> graphMap)
        {
            foreach (var graph in graphMap)
            {
                graphs.Add(new GraphData(graph.Key, graph.Value));
            }
        }

        public BehaviourGraph BuildSystem()
        {
            if (graphs.Count > 0)
            {
                for (int i = 0; i < graphs.Count; i++)
                {
                    graphs[i].Build(this);
                }

                return graphs[0].graph;
            }
            else
            {
                return null;
            }
        }

        public SystemData GetRuntimeCopy() => (SystemData)Clone();

        public object Clone()
        {
            var copy = new SystemData();
            copy.graphs = new List<GraphData>(graphs.Count);
            copy.pushPerceptions = new List<PushPerceptionData>(pushPerceptions.Count);

            for (int i = 0; i < graphs.Count; i++)
            {
                copy.graphs.Add((GraphData)graphs[i].Clone());
            }

            for (int i = 0; i < pushPerceptions.Count; i++)
            {
                copy.pushPerceptions.Add((PushPerceptionData)pushPerceptions[i].Clone());
            }

            return copy;
        }
    }
}

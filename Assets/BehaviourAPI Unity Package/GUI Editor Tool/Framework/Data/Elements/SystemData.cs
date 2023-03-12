using BehaviourAPI.Core;
using BehaviourAPI.UnityTool.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [Serializable]
    public class SystemData : ICloneable
    {
        [HideInInspector] public List<GraphData> graphs = new List<GraphData>();
        [HideInInspector] public List<PushPerceptionData> pushPerception = new List<PushPerceptionData>();

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

        public object Clone()
        {
            var copy = new BehaviourSystemData();
            copy.graphs = new List<GraphData>(graphs.Count);
            copy.pushPerception = new List<PushPerceptionData>(pushPerception.Count);

            for (int i = 0; i < graphs.Count; i++)
            {
                copy.graphs.Add((GraphData)graphs[i].Clone());
            }

            for (int i = 0; i < pushPerception.Count; i++)
            {
                copy.pushPerception.Add((PushPerceptionData)pushPerception[i].Clone());
            }

            return copy;
        }
    }
}

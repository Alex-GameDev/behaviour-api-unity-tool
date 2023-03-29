using BehaviourAPI.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Class that serializes a behaviour system
    /// </summary>
    [Serializable]
    public class SystemData : ICloneable
    {
        /// <summary>
        /// List of the graphs stored in the system. The first node is the main one.
        /// </summary>
        [HideInInspector] public List<GraphData> graphs = new List<GraphData>();

        /// <summary>
        /// List of push perceptions.
        /// </summary>
        [HideInInspector] public List<PushPerceptionData> pushPerceptions = new List<PushPerceptionData>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SystemData()
        {
        }

        /// <summary>
        /// Create a new <see cref="SystemData"/> by a collection of named <see cref="BehaviourGraph"/>.
        /// </summary>
        /// <param name="graphMap"></param>
        public SystemData(Dictionary<BehaviourGraph, string> graphMap)
        {
            foreach (var graph in graphMap)
            {
                graphs.Add(new GraphData(graph.Key, graph.Value));
            }
        }

        /// <summary>
        /// Build the main <see cref="BehaviourGraph"/> using the serialized data.
        /// </summary>
        /// <returns>The main <see cref="BehaviourGraph"/> that will be executed.</returns>
        public BehaviourGraph BuildSystem()
        {
            BehaviourGraph maingraph;
            if (graphs.Count > 0)
            {
                for (int i = 0; i < graphs.Count; i++)
                {
                    graphs[i].Build(this);
                }

                maingraph = graphs[0].graph;
            }
            else
            {
                maingraph = null;
            }

            foreach (var pushPerception in pushPerceptions)
            {
                pushPerception.Build(this);
            }

            return maingraph;
        }

        /// <summary>
        /// Get a copy of the system data to be executed.
        /// </summary>
        /// <returns>The copy of the system data.</returns>
        public SystemData GetRuntimeCopy() => (SystemData)Clone();

        /// <summary>
        /// Get a deep copy of the graph data
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
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

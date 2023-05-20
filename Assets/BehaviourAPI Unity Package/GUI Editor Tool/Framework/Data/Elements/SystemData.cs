using BehaviourAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Class that serializes a behaviour system
    /// </summary>
    [Serializable]
    public class SystemData
    {
        /// <summary>
        /// List of the graphs stored in the system. The first node is the main one.
        /// </summary>
        public List<GraphData> graphs = new List<GraphData>();

        /// <summary>
        /// List of push perceptions.
        /// </summary>
        public List<PushPerceptionData> pushPerceptions = new List<PushPerceptionData>();

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
        public BehaviourGraph BuildSystem(Component runner)
        {
            BehaviourGraph maingraph;
            if (graphs.Count > 0)
            {
                for (int i = 0; i < graphs.Count; i++)
                {
                    graphs[i].Build(this, runner);
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

        public Dictionary<string, NodeData> GetNodeIdMap()
        {
            var dict = new Dictionary<string, NodeData>();
            foreach (GraphData graph in graphs)
            {
                foreach (NodeData node in graph.nodes)
                {
                    dict.Add(node.id, node);
                }
            }
            return dict;
        }
    }
}

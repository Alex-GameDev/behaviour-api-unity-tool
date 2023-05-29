using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
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
        /// <returns>The <see cref="BuildedSystemData"/>.</returns>
        public BuildedSystemData BuildSystem(Component runner)
        {
            BuildData buildData = new BuildData(runner, this);

            Dictionary<string, BehaviourGraph> graphMap = new Dictionary<string, BehaviourGraph>();

            for(int i = 0; i < graphs.Count; i++)
            {
                graphs[i].Build(buildData);
                if (!string.IsNullOrWhiteSpace(graphs[i].name))
                {
                    if (!graphMap.TryAdd(graphs[i].name, graphs[i].graph))
                        Debug.LogWarning($"BUILD WARNING: Graph \"{graphs[i].name}\" wasn't added to dictionary because a graph with the same name was added before.", runner);
                }
                else
                {
                    Debug.LogWarning($"BUILD WARNING: Graph \"{graphs[i].name}\" wasn't added to dictionary because the name is not valid", runner);
                }
            }

            Dictionary<string, PushPerception> pushPerceptionMap = new Dictionary<string, PushPerception>();

            for (int i = 0; i < pushPerceptions.Count; i++)
            {
                pushPerceptions[i].Build(buildData);
                if (!string.IsNullOrWhiteSpace(pushPerceptions[i].name))
                {
                    if (!pushPerceptionMap.TryAdd(pushPerceptions[i].name, pushPerceptions[i].pushPerception))
                    {
                        Debug.LogWarning($"ERROR: Push perception \"{pushPerceptions[i].name}\" wasn't added to dictionary because a push perception with the same name was added before.", runner);
                    }
                }
                else
                {
                    Debug.LogWarning($"ERROR: Push perception \"{pushPerceptions[i].name}\" wasn't added to dictionary because the name is not valid", runner);
                }
            }

            BehaviourGraph mainGraph = graphs.FirstOrDefault()?.graph;

            BuildedSystemData buildedSystemData = new BuildedSystemData(mainGraph, graphMap, pushPerceptionMap);
            return buildedSystemData;
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

        public bool ValidateReferences()
        {
            bool referencesChanged = false;
            foreach(GraphData graphData in graphs)
            {
                referencesChanged |= graphData.ValidateReferences();
            }
            return referencesChanged;
        }
    }
}

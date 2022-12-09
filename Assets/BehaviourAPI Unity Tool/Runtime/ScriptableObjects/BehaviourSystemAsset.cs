using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a system compound by multiple behaviour graphs as an unity object
    /// </summary>
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourSystemAsset : ScriptableObject
    {
        [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();

        public GraphAsset RootGraph
        {
            get
            {
                if(graphs.Count == 0) return null;
                else return graphs[0];
            }
            set
            {
                if (graphs.Count > 0)
                    graphs.MoveAtFirst(value);
            }
        }

        public List<GraphAsset> Graphs
        {
            get => graphs;
        }

        public GraphAsset CreateGraph(string name, Type type)
        {
            var graphAsset = GraphAsset.Create(name, type);

            if(graphAsset != null)
            {
                Graphs.Add(graphAsset);
            }
            return graphAsset;
        }

        public void RemoveGraph(GraphAsset graph)
        {
            graphs.Remove(graph);
        }
    }
}
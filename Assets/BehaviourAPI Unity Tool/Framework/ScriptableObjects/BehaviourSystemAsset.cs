using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Stores a system compound by multiple behaviour graphs as an unity object
    /// </summary>
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourSystemAsset : ScriptableObject
    {
        [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();
        [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();

        Dictionary<string, PushPerception> buildedPushPerceptions;

        public GraphAsset MainGraph
        {
            get
            {
                if (graphs.Count == 0) return null;
                else return graphs[0];
            }
            set
            {
                if (graphs.Contains(value))
                    graphs.MoveAtFirst(value);
            }
        }

        public List<GraphAsset> Graphs => graphs;
        public List<PushPerceptionAsset> PushPerceptions => pushPerceptions;


        public GraphAsset CreateGraph(string name, Type type)
        {
            var graphAsset = GraphAsset.Create(name, type);

            if (graphAsset != null)
            {
                Graphs.Add(graphAsset);
            }
            return graphAsset;
        }

        public PushPerceptionAsset CreatePushPerception(string name)
        {
            var pushPerceptionAsset = PushPerceptionAsset.Create(name);

            if (pushPerceptionAsset != null)
            {
                PushPerceptions.Add(pushPerceptionAsset);
            }
            return pushPerceptionAsset;
        }

        public void RemoveGraph(GraphAsset graph)
        {
            Graphs.Remove(graph);
        }

        public void RemovePushPerception(PushPerceptionAsset pushPerception)
        {
            PushPerceptions.Remove(pushPerception);
        }

        public BehaviourGraph Build()
        {
            graphs.ForEach(g => g.Build());

            buildedPushPerceptions = new Dictionary<string, PushPerception>();
            pushPerceptions.ForEach(pp =>
            {
                if(pp.Targets.Count > 0)
                {
                    if (!buildedPushPerceptions.TryAdd(pp.Name, pp.Build()))
                    {
                        Debug.LogWarning($"Push perception with name \"{pp.Name}\" cannot be added. Another push perception with the same name already exists");
                    }
                }
                else
                {
                    Debug.Log($"Push perception with name \"{pp.Name}\" wasn't added because it has no targets.");
                    return;
                }
            });

            return MainGraph?.Graph ?? null;
        }

        public PushPerception GetPushPerception(string name)
        {
            return buildedPushPerceptions.GetValueOrDefault(name);
        }

        // ----------------------------------------------

        public static BehaviourSystemAsset CreateSystem(HashSet<BehaviourGraph> behaviourGraphs)
        {
            var system = CreateInstance<BehaviourSystemAsset>();
            foreach(var graph in behaviourGraphs)
            {
                var name = graph.GetType().Name;
                var graphAsset = GraphAsset.Create(name, graph);
                system.graphs.Add(graphAsset);
            }
            return system;
        }
    }
}
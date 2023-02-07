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
        [SerializeField] List<PerceptionAsset> perceptions = new List<PerceptionAsset>();

        Dictionary<string, PushPerception> pushPerceptionMap;
        Dictionary<string, Perception> pullPerceptionMap;
        Dictionary<string, Dictionary<string, Node>> nodeMap;

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
        public List<PerceptionAsset> Perceptions => perceptions;

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

        public PerceptionAsset CreatePerception(string name, Type type)
        {
            var perceptionAsset = PerceptionAsset.Create(name, type);

            if (perceptionAsset != null)
            {
                Perceptions.Add(perceptionAsset);
            }
            return perceptionAsset;
        }

        public void RemoveGraph(GraphAsset graph)
        {
            Graphs.Remove(graph);
        }

        public void RemovePushPerception(PushPerceptionAsset pushPerception)
        {
            PushPerceptions.Remove(pushPerception);
        }

        public void RemovePerception(PerceptionAsset pushPerception)
        {
            Perceptions.Remove(pushPerception);
        }

        public BehaviourGraph Build()
        {
            graphs.ForEach(g => g.Build());
            BuildPushPerceptionMap();
            BuildPullPerceptionMap();
            return MainGraph?.Graph ?? null;
        }

        void BuildPushPerceptionMap()
        {
            pushPerceptionMap = new Dictionary<string, PushPerception>();
            foreach(var pushPerception in pushPerceptions)
            {
                if(pushPerception.Targets.Count > 0)
                {
                    if (!pushPerceptionMap.TryAdd(pushPerception.Name, pushPerception.pushPerception))
                    {
                        Debug.LogWarning($"Push perception with name \"{pushPerception.Name}\" cannot be added. Another push perception with the same name already exists");
                    }
                }
                else
                {
                    Debug.Log($"Push perception \"{pushPerception.Name}\" wasn't added because it has no targets.");
                }
            }
        }

        void BuildPullPerceptionMap()
        {

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

        #region ------------------------------ Find elements ------------------------------

        public PushPerception FindPushPerception(string name)
        {
            return pushPerceptionMap.GetValueOrDefault(name);
        }

        public Perception FindPerception(string name)
        {
            return pullPerceptionMap.GetValueOrDefault(name);
        }

        public T FindNode<T>(string nodeName, string graphName) where T : Node
        {
            if(graphName != null)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
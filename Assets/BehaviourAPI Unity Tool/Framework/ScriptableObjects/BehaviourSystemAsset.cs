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
    [CreateAssetMenu(fileName = "newBehaviourSystem", menuName = "BehaviourAPI/BehaviourSystem", order = 0)]
    public class BehaviourSystemAsset : ScriptableObject
    {
        [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();
        [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();
        [SerializeField] List<PerceptionAsset> perceptions = new List<PerceptionAsset>();

        public Dictionary<string, PushPerception> pushPerceptionMap;
        public Dictionary<string, Perception> pullPerceptionMap;
        public Dictionary<string, BehaviourGraph> graphMap;

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

        public BehaviourGraph Build(NamingSettings nodeSettings, NamingSettings perceptionSettings, NamingSettings pushSettings)
        {
            BuildPullPerceptionMap(perceptionSettings);
            BuildGraphMap(nodeSettings);
            BuildPushPerceptionMap(pushSettings);
            return MainGraph?.Graph ?? null;
        }

        void BuildGraphMap(NamingSettings settings)
        {
            graphs.ForEach(g => g.Build(settings));

            graphMap = new Dictionary<string, BehaviourGraph>();

            if(settings == NamingSettings.TryAddAlways)
            { 
                foreach(var graph in graphs)
                {
                    graphMap.Add(graph.Name, graph.Graph);
                }
            }
            else if(settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var graph in graphs)
                {
                    if (graph.Graph != null)
                    {
                        if (!graphMap.TryAdd(graph.Name, graph.Graph))
                        {
                            Debug.LogWarning($"Graph with name \"{graph.Name}\" cannot be added to map. Another graph with the same name was already added");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Graph \"{graph.Name}\" wasn't added because it's empty.");
                    }
                }
            }
        }

        void BuildPushPerceptionMap(NamingSettings settings)
        {
            pushPerceptions.ForEach(pp => pp.Build());

            pushPerceptionMap = new Dictionary<string, PushPerception>();
            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var pushPerception in pushPerceptions)
                {
                    pushPerceptionMap.Add(pushPerception.Name, pushPerception.pushPerception);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var pushPerception in pushPerceptions)
                {
                    if (pushPerception.Targets.Count > 0)
                    {
                        if (!pushPerceptionMap.TryAdd(pushPerception.Name, pushPerception.pushPerception))
                        {
                            Debug.LogWarning($"Push perception with name \"{pushPerception.Name}\" cannot be added. Another push perception with the same name was already added");
                        }
                    }
                    else
                    {
                        Debug.Log($"Push perception \"{pushPerception.Name}\" wasn't added because it has no targets.");
                    }
                }
            }
        }

        void BuildPullPerceptionMap(NamingSettings settings)
        {
            perceptions.ForEach(p => p.Build());

            pullPerceptionMap = new Dictionary<string, Perception>();
            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var perception in perceptions)
                {
                    pullPerceptionMap.Add(perception.Name, perception.perception);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var perception in perceptions)
                {
                    if (perception.perception != null)
                    {
                        if (!pullPerceptionMap.TryAdd(perception.Name, perception.perception))
                        {
                            Debug.LogWarning($"Perception with name \"{perception.Name}\" cannot be added. Another perception with the same name was already addeds");
                        }
                    }
                    else
                    {
                        Debug.Log($"Push perception \"{perception.Name}\" wasn't added because it's empty.");
                    }
                }
            }
        }

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
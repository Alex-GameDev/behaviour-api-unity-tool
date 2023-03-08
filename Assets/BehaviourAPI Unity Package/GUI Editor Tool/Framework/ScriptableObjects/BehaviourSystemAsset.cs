using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [CreateAssetMenu(fileName = "newBehaviourSystem", menuName = "BehaviourAPI/BehaviourSystem", order = 0)]
    public class BehaviourSystemAsset : ScriptableObject, IBehaviourSystem
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
        public List<PerceptionAsset> PullPerceptions => perceptions;

        public bool IsAsset => true;

        public Object ObjectReference => this;
        public ScriptableObject AssetReference => this;
        public Component ComponentReference => null;

        public static BehaviourSystemAsset CreateSystem(Dictionary<BehaviourGraph, string> behaviourGraphs)
        {
            var system = CreateInstance<BehaviourSystemAsset>();
            foreach (var kvp in behaviourGraphs)
            {
                var name = !string.IsNullOrEmpty(kvp.Value) ? kvp.Value : kvp.Key.GetType().Name;
                var graphAsset = GraphAsset.Create(name, kvp.Key);
                system.graphs.Add(graphAsset);
            }
            return system;
        }

        public static BehaviourSystemAsset CreateSystem(List<GraphAsset> graphs, List<PerceptionAsset> pullPerceptions, List<PushPerceptionAsset> pushPerceptions)
        {
            var system = CreateInstance<BehaviourSystemAsset>();
            system.graphs = graphs;
            system.pushPerceptions = pushPerceptions;
            system.perceptions = pullPerceptions;
            return system;
        }

        #region -------------------------------- Runtime --------------------------------
        public void Build(NamingSettings nodeSettings, NamingSettings perceptionSettings, NamingSettings pushSettings)
        {
            BuildPullPerceptionMap(perceptionSettings);
            BuildGraphMap(nodeSettings);
            BuildPushPerceptionMap(pushSettings);
        }

        void BuildGraphMap(NamingSettings settings)
        {
            graphs.ForEach(g => g.Build(settings));

            graphMap = new Dictionary<string, BehaviourGraph>();

            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var graph in graphs)
                {
                    graphMap.Add(graph.Name, graph.Graph);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
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

        #endregion
    }
}
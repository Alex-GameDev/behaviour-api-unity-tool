using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class AssetBehaviourRunner : BehaviourRunner
    {
        public NamingSettings nodeNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings perceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings pushPerceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        public BehaviourSystemAsset System;

        Dictionary<string, PushPerception> pushPerceptionMap;
        Dictionary<string, Perception> pullPerceptionMap;
        Dictionary<string, BehaviourGraph> graphMap;

        protected BehaviourGraph MainGraph;

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return System;
        }

        protected override void OnAwake()
        {
            MainGraph = Build(nodeNamingSettings, perceptionNamingSettings, pushPerceptionNamingSettings);
        }

        protected override void OnStart()
        {
            MainGraph?.Start();
        }

        protected override void OnUpdate()
        {
            MainGraph?.Update();
        }

        public BehaviourGraph Build(NamingSettings nodeSettings, NamingSettings perceptionSettings, NamingSettings pushSettings)
        {
            BuildPullPerceptionMap(perceptionSettings);
            BuildGraphMap(nodeSettings);
            BuildPushPerceptionMap(pushSettings);

            return System.MainGraph?.Graph ?? null;
        }

        private void OnDisable()
        {
            MainGraph?.Stop();
        }

        void BuildGraphMap(NamingSettings settings)
        {
            System.Graphs.ForEach(g => g.Build(settings));

            graphMap = new Dictionary<string, BehaviourGraph>();

            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var graph in System.Graphs)
                {
                    graphMap.Add(graph.Name, graph.Graph);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var graph in System.Graphs)
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
            System.PushPerceptions.ForEach(pp => pp.Build());

            pushPerceptionMap = new Dictionary<string, PushPerception>();
            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var pushPerception in System.PushPerceptions)
                {
                    pushPerceptionMap.Add(pushPerception.Name, pushPerception.pushPerception);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var pushPerception in System.PushPerceptions)
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
            System.PullPerceptions.ForEach(p => p.Build());

            pullPerceptionMap = new Dictionary<string, Perception>();
            if (settings == NamingSettings.TryAddAlways)
            {
                foreach (var perception in System.PullPerceptions)
                {
                    pullPerceptionMap.Add(perception.Name, perception.perception);
                }
            }
            else if (settings == NamingSettings.IgnoreWhenInvalid)
            {
                foreach (var perception in System.PullPerceptions)
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
    }
}

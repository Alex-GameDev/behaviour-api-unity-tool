using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEditor;
using UnityEditor.Callbacks;
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
        [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();

        Dictionary<string, PushPerception> buildedPushPerceptions;

        public GraphAsset RootGraph
        {
            get
            {
                if (graphs.Count == 0) return null;
                else return graphs[0];
            }
            set
            {
                if (graphs.Count > 0)
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

            //buildedPushPerceptions = pushPerceptions.ToDictionary(p => p.Name, p => p.Build());
            return RootGraph?.Graph ?? null;
        }

        public PushPerception GetPushPerception(string name)
        {
            return buildedPushPerceptions.GetValueOrDefault(name);
        }
    }
}
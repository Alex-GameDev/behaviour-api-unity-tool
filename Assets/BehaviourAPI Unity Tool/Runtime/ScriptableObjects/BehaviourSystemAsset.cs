using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a system compound by multiple behaviour graphs as an unity object
    /// </summary>
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourSystemAsset : ScriptableObject
    {
        [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();

        [SerializeField] List<PerceptionAsset> perceptions = new List<PerceptionAsset>();
        [SerializeField] List<ActionAsset> actions = new List<ActionAsset>();
        [SerializeField] List<PushPerceptionAsset> pushPerceptions = new List<PushPerceptionAsset>();

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
        public List<PerceptionAsset> Perceptions => perceptions;
        public List<ActionAsset> Actions => actions;
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

        public ActionAsset CreateAction(string name, Type type)
        {
            var actionAsset = ActionAsset.Create(name, type);

            if (actionAsset != null)
            {
                Actions.Add(actionAsset);
            }
            return actionAsset;
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

        public void RemoveAction(ActionAsset action)
        {
            Actions.Remove(action);
        }

        public void RemovePerception(PerceptionAsset perception)
        {
            Perceptions.Remove(perception);
        }

        public void RemovePushPerception(PushPerceptionAsset pushPerception)
        {
            PushPerceptions.Remove(pushPerception);
        }
    }
}
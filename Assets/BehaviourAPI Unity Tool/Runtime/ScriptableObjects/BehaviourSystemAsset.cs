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
        [SerializeField] GraphAsset rootGraph;
        [SerializeField] List<GraphAsset> graphs = new List<GraphAsset>();

        [SerializeField] List<ActionAsset> actions = new List<ActionAsset>();

        public GraphAsset RootGraph
        {
            get => rootGraph;
            set => rootGraph = value;
        }

        public List<ActionAsset> Actions
        {
            get => actions;
        }

        public List<GraphAsset> Graphs
        {
            get => graphs;
        }

        public ActionAsset CreateAction(Type type)
        {
            var actionAsset = ActionAsset.Create(type);

            if (actionAsset != null)
            {
                Actions.Add(actionAsset);
            }

            return actionAsset;
        }

        public GraphAsset CreateGraph(Type type)
        {
            var graphAsset = GraphAsset.Create(type);

            if(graphAsset != null)
            {
                Graphs.Add(graphAsset);
            }
            return graphAsset;
        }

        public void CreateRootGraph(Type graphType)
        {
            var asset = CreateGraph(graphType);
            RootGraph = asset;
        }
    }
}
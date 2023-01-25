using BehaviourAPI.Core;
using BehaviourAPI.Core.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Stores a behaviour graph as an unity object.
    /// </summary>
    public class GraphAsset : ScriptableObject
    {
        public string Name;

        [SerializeReference] BehaviourGraph graph;
        [HideInInspector][SerializeField] List<NodeAsset> nodes = new List<NodeAsset>();

        public BehaviourGraph Graph
        {
            get => graph;
            set => graph = value;
        }

        public List<NodeAsset> Nodes
        {
            get => nodes;
        }

        public NodeAsset CreateNode(Type type, Vector2 position)
        {
            if (Graph == null) return null;

            if (!type.IsSubclassOf(Graph.NodeType)) return null;

            var nodeasset = NodeAsset.Create(type, position);
            Nodes.Add(nodeasset);
            return nodeasset;
        }

        public void RemoveNode(NodeAsset node)
        {
            Nodes.Remove(node);
        }

        public static GraphAsset Create(string name, Type graphType)
        {
            var graphAsset = CreateInstance<GraphAsset>();
            graphAsset.Graph = (BehaviourGraph)Activator.CreateInstance(graphType);
            graphAsset.Name = name;
            return graphAsset;
        }

        public BehaviourGraph Build()
        {
            var graphBuilder = new BehaviourGraphBuilder(graph);
            nodes.ForEach(n => graphBuilder.AddNode(n.Build()));
            graphBuilder.Build();
            return graph;
        }
    }
}

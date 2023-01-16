using BehaviourAPI.Core;
using BehaviourAPI.Core.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
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

        public BehaviourGraph Build() => graph;

        //public void OnBeforeSerialize()
        //{
        //    return;
        //}

        //public void OnAfterDeserialize()
        //{
        //    var graphBuilder = new BehaviourGraphBuilder(graph);
        //    nodes.ForEach(n => graphBuilder.AddNode(n.Build()));
        //    graphBuilder.Build();
        //}

        public GraphAsset Clone()
        {
            GraphAsset graphAsset = CreateInstance<GraphAsset>();
            graphAsset.graph = (BehaviourGraph) Activator.CreateInstance(graph.GetType());

            var map = new Dictionary<NodeAsset, NodeAsset>();

            for(int i = 0; i < Nodes.Count; i++)
            {
                NodeAsset nodeCopy = Nodes[i].Clone();
                graphAsset.Nodes.Add(nodeCopy);
                map.Add(Nodes[i], nodeCopy);
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                graphAsset.Nodes[i].Parents = nodes[i].Parents.Select(p => map.GetValueOrDefault(p)).ToList();
                graphAsset.Nodes[i].Childs = nodes[i].Childs.Select(p => map.GetValueOrDefault(p)).ToList();
            }

            return graphAsset;
        }
    }
}

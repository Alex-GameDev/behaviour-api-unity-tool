using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
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

        public static GraphAsset Create(string name, BehaviourGraph behaviourGraph)
        {
            var graphAsset = CreateInstance<GraphAsset>();
            graphAsset.Graph = behaviourGraph;
            graphAsset.Name = name;

            var nodeAssetMap = new Dictionary<Node, NodeAsset>();

            foreach (var node in behaviourGraph.NodeList)
            {
                var nodeAsset = NodeAsset.Create(node, Vector2.zero);
                nodeAssetMap[node] = nodeAsset;
                graphAsset.Nodes.Add(nodeAsset);
            }

            foreach (var node in graphAsset.Nodes)
            {
                for (int i = 0; i < node.Node.ParentCount; i++)
                {
                    node.Parents.Add(nodeAssetMap[node.Node.GetParentAt(i)]);
                }

                for (int i = 0; i < node.Node.ChildCount; i++)
                {
                    node.Childs.Add(nodeAssetMap[node.Node.GetChildAt(i)]);
                }
            }

            //LayoutUtilities.ComputeLayout(graphAsset);
            LayoutUtilities.ComputeLayout(graphAsset);
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

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

        public NodeAsset DuplicateNode(NodeAsset nodeAsset)
        {
            if (Graph == null) return null;

            if (!Nodes.Contains(nodeAsset)) return null;

            var nodeasset = nodeAsset.Clone();
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

            var nodeNameMap = behaviourGraph.GetNodeNames();

            var nodeAssetMap = new Dictionary<Node, NodeAsset>();

            foreach (var node in behaviourGraph.NodeList)
            {
                var nodeAsset = NodeAsset.Create(node, Vector2.zero);
                nodeAssetMap[node] = nodeAsset;

                if(nodeNameMap.TryGetValue(node, out var nodeName))
                {
                    nodeAsset.Name = nodeName;
                }
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

        public GraphAsset Copy()
        {
            var graphAsset = CreateInstance<GraphAsset>();
            graphAsset.graph = (BehaviourGraph) graph.Clone();

            var assetMap = new Dictionary<NodeAsset, NodeAsset>();

            foreach(var node in nodes)
            {
                var clone = node.Clone();
                graphAsset.nodes.Add(clone);
                assetMap.Add(node, clone);
            }

            foreach (var node in graphAsset.Nodes)
            {
                for (int i = 0; i < node.Node.ParentCount; i++)
                {
                    node.Parents.Add(assetMap[node.Parents[i]]);
                }

                for (int i = 0; i < node.Node.ChildCount; i++)
                {
                    node.Childs.Add(assetMap[node.Childs[i]]);
                }
            }

            return graphAsset;
        }

        public BehaviourGraph Build(NamingSettings settings)
        {
            var graphBuilder = new BehaviourGraphBuilder(graph);
            if(settings == NamingSettings.TryAddAlways)
            {
                nodes.ForEach(n => graphBuilder.AddNode(n.Name, n.Node, n.GetParentNodes(), n.GetChildNodes()));
            }
            else if(settings == NamingSettings.IgnoreWhenInvalid)
            {
                nodes.ForEach(n =>
                {
                    if (string.IsNullOrWhiteSpace(n.Name) || graph.FindNodeOrDefault(n.Name) != null)
                    {
                        graphBuilder.AddNode(n.Node, n.GetParentNodes(), n.GetChildNodes());
                    }
                    else
                    {
                        graphBuilder.AddNode(n.Name, n.Node, n.GetParentNodes(), n.GetChildNodes());
                    }
                });
            }
            else
            {
                nodes.ForEach(n => graphBuilder.AddNode(n.Name, n.Node, n.GetParentNodes(), n.GetChildNodes()));
            }

            graphBuilder.Build();
            return graph;
        }
    }
}

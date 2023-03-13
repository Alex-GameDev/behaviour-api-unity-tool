using System;
using System.Collections.Generic;

using System.Linq;

using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;
    using Core.Serialization;

    /// <summary>
    /// Stores a behaviour graph as an unity object.
    /// </summary>
    [Serializable]
    public class GraphData : ICloneable
    {
        [HideInInspector] public string id;
        public string name;
        [SerializeReference] public BehaviourGraph graph;
        [HideInInspector] public List<NodeData> nodes;

        public GraphData(Type graphType)
        {
            nodes = new List<NodeData>();
            graph = (BehaviourGraph)Activator.CreateInstance(graphType);
            nodes = new List<NodeData>();

            name = graphType.Name;
            id = Guid.NewGuid().ToString();
        }

        public GraphData()
        {
        }

        public Dictionary<string, NodeData> GetNodeIdMap()
        {
            return nodes.ToDictionary(n => n.id, n => n);
        }

        public void OrderChildNodes(NodeData nodeData, Func<NodeData, float> sortFunction)
        {
            var dict = GetNodeIdMap();
            nodeData.childIds = nodeData.childIds.OrderBy(id =>
            {
                if(dict.TryGetValue(id, out NodeData data))
                {
                    return sortFunction(data);
                }
                else
                {
                    return float.MaxValue;
                }
            }).ToList();
        }

        public void OrderAllChildNodes(Func<NodeData, float> sortFunction)
        {
            var dict = GetNodeIdMap();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].childIds = nodes[i].childIds.OrderBy(id =>
                {
                    if (dict.TryGetValue(id, out NodeData data))
                    {
                        return sortFunction(data);
                    }
                    else
                    {
                        return float.MaxValue;
                    }
                }).ToList();
            }
        }

        public HashSet<NodeData> GetChildPathing(NodeData start)
        {
            Dictionary<string, NodeData> nodeIdMap = GetNodeIdMap();
            HashSet<NodeData> visitedNodes = new HashSet<NodeData>();
            HashSet<NodeData> unvisitedNodes = new HashSet<NodeData>();

            unvisitedNodes.Add(start);
            while (unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);

                for (int i = 0; i < node.childIds.Count; i++)
                {
                    NodeData child = nodeIdMap[node.childIds[i]];

                    if (!visitedNodes.Contains(child))
                    {
                        unvisitedNodes.Add(child);
                    }
                }
            }
            return visitedNodes;
        }

        public HashSet<NodeData> GetParentPathing(NodeData start)
        {
            Dictionary<string, NodeData> nodeIdMap = GetNodeIdMap();
            HashSet<NodeData> visitedNodes = new HashSet<NodeData>();
            HashSet<NodeData> unvisitedNodes = new HashSet<NodeData>();

            unvisitedNodes.Add(start);
            while (unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);

                for (int i = 0; i < node.parentIds.Count; i++)
                {
                    NodeData child = nodeIdMap[node.parentIds[i]];

                    if (!visitedNodes.Contains(child))
                    {
                        unvisitedNodes.Add(child);
                    }
                }
            }
            return visitedNodes;
        }

        public void Build(SystemData data)
        {
            var builder = new BehaviourGraphBuilder(graph);
            var nodeIdMap = GetNodeIdMap();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].node is IBuildable buildable) buildable.Build(data);

                builder.AddNode(nodes[i].node,
                    nodes[i].parentIds.Select(id => nodeIdMap[id].node).ToList(),
                    nodes[i].childIds.Select(id => nodeIdMap[id].node).ToList()
                    );
            }
            builder.Build();
        }

        public object Clone()
        {
            GraphData copy = new GraphData();
            copy.id = id;
            copy.name = name;
            copy.nodes = new List<NodeData>(nodes.Count);

            for (int i = 0; i < nodes.Count; i++)
            {
                copy.nodes.Add((NodeData)nodes[i].Clone());
            }

            copy.graph = (BehaviourGraph)graph.Clone();
            return copy;
        }
    }
}

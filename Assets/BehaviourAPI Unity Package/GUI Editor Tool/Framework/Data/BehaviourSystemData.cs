using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Unity.Framework.Adaptations;
using behaviourAPI.Unity.Framework.Adaptations;
using Action = BehaviourAPI.Core.Actions.Action;
using UnityEditor;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Object = UnityEngine.Object;
using BehaviourAPI.Core.Serialization;

namespace BehaviourAPI.UnityTool.Framework
{
    [Serializable]
    public class BehaviourSystemData : ICloneable
    {
        [HideInInspector] public List<GraphData> graphs = new List<GraphData>();
        [HideInInspector] public List<PushPerceptionData> pushPerception = new List<PushPerceptionData>();

        public BehaviourGraph BuildSystem()
        {
            if(graphs.Count > 0)
            {
                for (int i = 0; i < graphs.Count; i++)
                {
                    graphs[i].Build();
                }

                return graphs[0].graph;
            }
            else
            {
                return null;
            }
        }

        public object Clone()
        {
            var copy = new BehaviourSystemData();
            copy.graphs = new List<GraphData>(graphs.Count);
            copy.pushPerception = new List<PushPerceptionData>(pushPerception.Count);

            for(int i = 0; i < graphs.Count; i++)
            {
                copy.graphs.Add((GraphData)graphs[i].Clone());
            }

            for (int i = 0; i < pushPerception.Count; i++)
            {
                copy.pushPerception.Add((PushPerceptionData)pushPerception[i].Clone());
            }

            return copy;
        }
    }

    [Serializable]
    public class GraphData : ICloneable
    {
        [HideInInspector] public string id;
        public string name;
        [HideInInspector] public List<NodeData> nodes;
        [SerializeReference] public BehaviourGraph graph;

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

        public HashSet<NodeData> GetChildPathing(NodeData start)
        {
            Dictionary<string, NodeData> nodeIdMap = GetNodeIdMap();
            HashSet<NodeData> visitedNodes = new HashSet<NodeData>();
            HashSet<NodeData> unvisitedNodes = new HashSet<NodeData>();

            unvisitedNodes.Add(start);
            while(unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);

                for(int i = 0; i < node.childIds.Count; i++)
                {
                    NodeData child = nodeIdMap[node.childIds[i]];

                    if(!visitedNodes.Contains(child))
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

        public void Build()
        {
            var builder = new BehaviourGraphBuilder(graph);
            var nodeIdMap = GetNodeIdMap();
            for (int i = 0; i < nodes.Count; i++)
            {
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

            for(int i = 0; i < nodes.Count; i++)
            {
                copy.nodes.Add((NodeData)nodes[i].Clone());
            }

            copy.graph = (BehaviourGraph)graph.Clone();
            return copy;
        }
    }

    [Serializable]
    public class NodeData : ICloneable
    {
        public string name;
        [HideInInspector] public string id;
        [HideInInspector] public UnityEngine.Vector2 position;

        [SerializeReference] public Node node;

        [HideInInspector] public List<string> parentIds;
        [HideInInspector] public List<string> childIds;

        public NodeData(Type type, UnityEngine.Vector2 position)
        {
            this.position = position;
            node = (Node)Activator.CreateInstance(type);
            parentIds = new List<string>();
            childIds = new List<string>();
            name = type.Name;
            id = Guid.NewGuid().ToString();
        }

        public NodeData()
        {
        }

        public object Clone()
        {
            NodeData copy = new NodeData();
            copy.name = name;
            copy.id = id;
            copy.position = position;
            copy.node = (Node)node.Clone();
            copy.parentIds = new List<string>(parentIds);
            copy.childIds = new List<string>(childIds);
            return copy;
        }
    }

    [Serializable]
    public class PushPerceptionData : ICloneable
    {
        public string name;
        [HideInInspector] public List<int> targetNodeIds;

        public object Clone()
        {
            PushPerceptionData copy = new PushPerceptionData();
            copy.name = name;
            copy.targetNodeIds = new List<int>(targetNodeIds);
            return copy;
        }
    }

    public interface IBuildable
    {
        public void Build(BehaviourSystemData data);
    }

    public class SubgraphAction : SubsystemAction, IBuildable
    {
        [HideInInspector] public string subgraphId;

        public SubgraphAction() : base(null, false, false) { }

        public void Build(BehaviourSystemData data)
        {
            // this.SubSystem = data.FindGraph(subgraphId);
        }
    }

    public class CustomAction : FunctionalAction
    {
        public ContextualSerializedAction start;
        public ContextualSerializedStatusFunction update;
        public ContextualSerializedAction stop;

        public CustomAction() : base(null)
        {
        }
    }

    public class CustomPerception : ConditionPerception
    {
        public ContextualSerializedAction init;
        public ContextualSerializedBoolFunction check;
        public ContextualSerializedAction reset;

        public CustomPerception() : base(null)
        {
        }
    }

    [Serializable]
    public class PerceptionWrapper : Perception
    {
        [SerializeReference] Perception perception;

        public override bool Check() => perception.Check();

        public override void Initialize() => perception.Initialize();

        public override void Reset() => perception.Reset();

        public override void SetExecutionContext(ExecutionContext context) => perception.SetExecutionContext(context);

        public override object Clone()
        {
            var p = (PerceptionWrapper) base.Clone();
            p.perception = (Perception) perception.Clone();
            return p;
        }
    }

    [Serializable]
    public class CompoundPerceptionWrapper : Perception
    {
        [SerializeReference] public CompoundPerception compoundPerception;
        public List<PerceptionWrapper> subPerceptions = new List<PerceptionWrapper>();


        public override bool Check() => compoundPerception.Check();

        public override void SetExecutionContext(ExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class LeafNode : BehaviourTrees.LeafNode, IActionAssignable
    {
        [SerializeReference] Action action;

        public Action ActionReference
        {
            get => action;
            set => action = value;
        }
        protected override void BuildConnections(List<Node> parents, List<Node> children)
        {
            base.BuildConnections(parents, children);
            Action = action;
        }
    }

    public class ConditionNode : BehaviourTrees.ConditionNode, IBuildable
    {
        [SerializeReference] public Perception perception;

        public void Build(BehaviourSystemData data)
        {
            // this.Perception = data.FindPerception(subgraphId);
        }
    }
}

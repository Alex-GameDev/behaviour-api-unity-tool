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

namespace BehaviourAPI.UnityTool.Framework
{
    [Serializable]
    public class BehaviourSystemData 
    {
        [HideInInspector] public List<GraphData> graphs;
        [HideInInspector] public List<PushPerceptionData> pushPerception;
    }

    [Serializable]
    public class GraphData
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
    }

    [Serializable]
    public class NodeData
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
    }

    [Serializable]
    public class PushPerceptionData
    {
        public string name;
        [HideInInspector] public List<int> targetNodeIds;
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

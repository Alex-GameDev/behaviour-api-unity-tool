using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourGraphAsset : ScriptableObject
    {
        [SerializeReference] BehaviourGraph graph;

        public BehaviourGraph Graph 
        { 
            get => graph;
            set => graph = value; 
        }

        public NodeAsset CreateNode(Type type, Vector2 position)
        {
            if(Graph == null) return null;

            if (!type.IsSubclassOf(Graph.NodeType)) return null;

            var nodeasset = NodeAsset.Create(type, position);
            return nodeasset;
        }

        public void Clear()
        {
            graph = null;
        }

        public void BindGraph(Type type)
        {
            graph = (BehaviourGraph)Activator.CreateInstance(type);
        }

        public static BehaviourGraphAsset Create(Type type, string name)
        {
            var graphAsset = CreateInstance<BehaviourGraphAsset>();
            graphAsset.name = name;
            graphAsset.graph = (BehaviourGraph)Activator.CreateInstance(type);
            return graphAsset;
        }
    }
}
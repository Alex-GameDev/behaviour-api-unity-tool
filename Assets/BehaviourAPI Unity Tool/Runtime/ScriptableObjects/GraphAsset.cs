using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        [SerializeReference] BehaviourGraph graph;
        [SerializeField] string Name;

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
            AssetDatabase.AddObjectToAsset(nodeasset, this);
            AssetDatabase.SaveAssets();
            return nodeasset;
        }

        public void RemoveNode(NodeAsset node)
        {

        }

        public static GraphAsset Create(string name, Type graphType)
        {
            var graphAsset = CreateInstance<GraphAsset>();
            graphAsset.Graph = (BehaviourGraph)Activator.CreateInstance(graphType);
            graphAsset.Name = name;
            return graphAsset;
        }
    }
}

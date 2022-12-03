using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourGraphAsset : ScriptableObject
    {
        [SerializeReference] BehaviourGraph graph;

        [SerializeField] List<NodeAsset> nodes;

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
            if(Graph == null) return null;

            if (!type.IsSubclassOf(Graph.NodeType)) return null;

            var nodeasset = NodeAsset.Create(type, position);
            Nodes.Add(nodeasset);
            AssetDatabase.AddObjectToAsset(nodeasset, this);
            AssetDatabase.SaveAssets();
            return nodeasset;
        }

        public void RemoveNode(NodeAsset node)
        {
            if (Graph == null) return;

            Nodes.Remove(node);
            AssetDatabase.RemoveObjectFromAsset(node);
            AssetDatabase.SaveAssets();
        }

        public void Clear()
        {
            graph = null;
        }

        public void BindGraph(Type type)
        {
            graph = (BehaviourGraph)Activator.CreateInstance(type);
        }
    }
}
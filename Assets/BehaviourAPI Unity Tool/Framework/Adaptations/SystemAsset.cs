using BehaviourAPI.BehaviourTrees;
using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [CreateAssetMenu(menuName = "A")]
    public class SystemAsset : ScriptableObject
    {
        [SerializeField] List<GraphData> graphs = new List<GraphData>();

        [ContextMenu("Test")]
        public void SetExample()
        {
            graphs.Add(new GraphData()
            {
                nodes = new List<NodeData>()
            {
                new NodeData()
                {
                    node = new Framework.Adaptations.LeafNode(),
                    Id = 1
                },
                new NodeData()
                {
                    node = new IteratorNode()
                    {
                        Iterations = 5
                    },
                    Id = 2
                }
            },
                graph = new BehaviourTree()

            });
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }

    [Serializable]
    public class GraphData
    {
        [SerializeReference] public BehaviourGraph graph;

        [SerializeField] public List<NodeData> nodes = new List<NodeData>();
    }

    [Serializable]
    public class NodeData
    {
        [SerializeField] public int Id;
        [SerializeReference] public Node node;
        [SerializeField] public List<int> parentIds = new List<int>();
        [SerializeField] public List<int> childrenIds = new List<int>();
    }

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a node as an unity object
    /// </summary>
    public class NodeAsset : ScriptableObject
    {
        public string Name;

        [SerializeReference] Node node;

        [HideInInspector][SerializeField] Vector2 position;
        [HideInInspector][SerializeField] List<NodeAsset> parents = new List<NodeAsset>();
        [HideInInspector][SerializeField] List<NodeAsset> childs = new List<NodeAsset>();

        public Node Node { get => node; set => node = value; }

        public Vector2 Position { get => position; set => position = value; }
        public List<NodeAsset> Parents { get => parents; private set => parents = value; }
        public List<NodeAsset> Childs { get => childs; private set => childs = value; }

        public static NodeAsset Create(Type type, Vector2 pos)
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.Position = pos;
            nodeAsset.Node = (Node)Activator.CreateInstance(type);
            return nodeAsset;
        }
    }
}
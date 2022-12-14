using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
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

        // Only if node handles an action
        [SerializeField] ActionAsset action;

        [SerializeField] Status ExitStatus = Status.None;

        [SerializeField] GraphAsset Subgraph;

        [SerializeField] bool executeOnLoop;
        [SerializeField] bool dontStopOnInterrupt;

        // Only if node handles an perception
        [SerializeField] PerceptionAsset perception;

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Serialization;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Framework
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

        public static NodeAsset Create(Node node, Vector2 pos)
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.Position = pos;
            nodeAsset.Node = node;
            return nodeAsset;
        }

        public void OrderChilds(Func<NodeAsset, float> shortFunction)
        {
            childs = childs.OrderBy(shortFunction).ToList();
        }

        internal NodeData Build()
        {
            return new NodeData(Node, parents.Select(p => p.Node).ToList(), childs.Select(c => c.Node).ToList());
        }

        /// <summary>
        /// Returns all the child nodes above this, including itself.
        /// </summary>
        public HashSet<NodeAsset> GetPathFromRoot()
        {
            HashSet<NodeAsset> visitedNodes = new HashSet<NodeAsset>();
            HashSet<NodeAsset> unvisitedNodes = new HashSet<NodeAsset>();

            unvisitedNodes.Add(this);
            while (unvisitedNodes.Count > 0)
            {

                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);
                node.Parents.ForEach(c =>
                {
                    if (!visitedNodes.Contains(c))
                        unvisitedNodes.Add(c);
                });
            }
            return visitedNodes;
        }

        /// <summary>
        /// Returns all the child nodes below this, including itself.
        /// </summary>
        public HashSet<NodeAsset> GetPathToLeaves()
        {
            HashSet<NodeAsset> visitedNodes = new HashSet<NodeAsset>();
            HashSet<NodeAsset> unvisitedNodes = new HashSet<NodeAsset>();

            unvisitedNodes.Add(this);
            while (unvisitedNodes.Count > 0)
            {
                var node = unvisitedNodes.First();
                unvisitedNodes.Remove(node);
                visitedNodes.Add(node);
                node.Childs.ForEach(c =>
                {
                    if (!visitedNodes.Contains(c))
                        unvisitedNodes.Add(c);
                });
            }
            return visitedNodes;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;

    /// <summary>
    /// Class to serialize node data
    /// </summary>
    [Serializable]
    public class NodeData : ICloneable
    {
        public string name;
        [HideInInspector] public string id;
        [HideInInspector] public UnityEngine.Vector2 position;

        [SerializeReference] public Node node;

        [HideInInspector] public List<string> parentIds = new List<string>();
        [HideInInspector] public List<string> childIds = new List<string>();

        public NodeData(Type type, UnityEngine.Vector2 position)
        {
            this.position = position;
            node = (Node)Activator.CreateInstance(type);
            name = type.Name;
            id = Guid.NewGuid().ToString();
        }

        public NodeData()
        {
        }

        public NodeData(Node node, string id)
        {
            this.node = node;
            this.id = id;
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

        /// <summary>
        /// Creates a copy in the same graph, changing the id and name
        /// </summary>
        /// <returns></returns>
        public NodeData Duplicate()
        {
            NodeData duplicate = new NodeData();
            duplicate.name = name + " (copy)";
            duplicate.id = Guid.NewGuid().ToString();
            duplicate.position = position + UnityEngine.Vector2.one * 50;
            duplicate.node = (Node)node.Clone();
            return duplicate;
        }
    }
}
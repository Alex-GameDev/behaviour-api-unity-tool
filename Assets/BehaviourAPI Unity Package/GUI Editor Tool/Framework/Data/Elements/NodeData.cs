using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core;

    /// <summary>
    /// Class that serialize node data.
    /// </summary>
    [Serializable]
    public class NodeData : ICloneable
    {

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string name;

        /// <summary>
        /// The unique id of this element.
        /// </summary>
        [HideInInspector] public string id;

        /// <summary>
        /// The position of the node in the editor.
        /// </summary>
        [HideInInspector] public UnityEngine.Vector2 position;

        /// <summary>
        /// The serializable reference of the node.
        /// </summary>
        [SerializeReference] public Node node;

        /// <summary>
        /// List of parent nodes referenced by id.
        /// </summary>
        [HideInInspector] public List<string> parentIds = new List<string>();

        /// <summary>
        /// List of children nodes referenced by id.
        /// </summary>
        [HideInInspector] public List<string> childIds = new List<string>();

        public NodeData(Type type, UnityEngine.Vector2 position)
        {
            this.position = position;
            node = (Node)Activator.CreateInstance(type);
            name = type.Name;
            id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NodeData()
        {
        }

        /// <summary>
        /// Create a new node data by a node and id.
        /// </summary>
        /// <param name="node">The <see cref="Node"/> reference</param>
        /// <param name="id">The id of the element.</param>
        public NodeData(Node node, string id)
        {
            this.node = node;
            this.id = id;
        }

        /// <summary>
        /// Creates a copy in the same graph, with a new id and name.
        /// </summary>
        /// <returns>The duplicated node.</returns>
        public NodeData Duplicate()
        {
            NodeData duplicate = new NodeData();
            duplicate.name = name + " (copy)";
            duplicate.id = Guid.NewGuid().ToString();
            duplicate.position = position + UnityEngine.Vector2.one * 50;
            duplicate.node = (Node)node.Clone();
            return duplicate;
        }

        /// <summary>
        /// Create a copy of the node data. 
        /// Used to create a runtime copy.
        /// </summary>
        /// <returns>A deep copy of the data.</returns>
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
}
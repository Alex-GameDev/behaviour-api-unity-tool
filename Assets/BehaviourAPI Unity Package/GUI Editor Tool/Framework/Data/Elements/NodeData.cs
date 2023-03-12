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

        public NodeData()
        {
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
    }
}
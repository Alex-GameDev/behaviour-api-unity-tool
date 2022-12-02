using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class NodeAsset : ScriptableObject
    {
        [SerializeReference] Node Node;

        public void SetNode(Node node)
        {
            Node = node;
        }

        public static NodeAsset Create<T>(string name) where T : Node, new()
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.name = name;
            nodeAsset.Node = new T();
            return nodeAsset;
        }

        public static NodeAsset Create(Type type, string name)
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.name = name;
            nodeAsset.Node = (Node)Activator.CreateInstance(type);
            return nodeAsset;
        }
    }
}
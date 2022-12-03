using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace BehaviourAPI.Unity.Runtime
{
    public class NodeAsset : ScriptableObject
    {
        [SerializeReference] Node node;

        [HideInInspector][SerializeField] Vector2 position;

        public Node Node
        {
            get => node;
            set => node = value;
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public static NodeAsset Create(Type type, Vector2 pos)
        {
            var nodeAsset = CreateInstance<NodeAsset>();
            nodeAsset.Position = pos;
            nodeAsset.Node = (Node)Activator.CreateInstance(type);
            return nodeAsset;
        }
    }
}
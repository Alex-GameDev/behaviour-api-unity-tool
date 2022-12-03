using System;
using System.Collections.Generic;
using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourGraphAsset : ScriptableObject
    {
        [SerializeReference] BehaviourGraph graph;

        public BehaviourGraph Graph { get => graph; set => graph = value; }

        public static BehaviourGraphAsset Create<T>(string name) where T : BehaviourGraph, new()
        {
            var graphAsset = CreateInstance<BehaviourGraphAsset>();
            graphAsset.name = name;
            graphAsset.graph = new T();
            return graphAsset;
        }

        public static BehaviourGraphAsset Create(Type type, string name)
        {
            var graphAsset = CreateInstance<BehaviourGraphAsset>();
            graphAsset.name = name;
            graphAsset.graph = (BehaviourGraph)Activator.CreateInstance(type);
            return graphAsset;
        }
    }
}
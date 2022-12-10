using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public abstract class PerceptionAsset : ScriptableObject
    {
        public string Name;

        [HideInInspector][SerializeField] List<NodeAsset> handlers;

        public List<NodeAsset> Handlers
        {
            get => handlers;
        }

        public static PerceptionAsset Create(string name, Type type)
        {
            if (!type.IsSubclassOf(typeof(PerceptionAsset))) return null;

            var perceptionAsset = (PerceptionAsset)CreateInstance(type);
            perceptionAsset.Name = name;
            return perceptionAsset;
        }
    }
}

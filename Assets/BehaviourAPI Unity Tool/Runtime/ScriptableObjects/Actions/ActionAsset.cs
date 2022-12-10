using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public abstract class ActionAsset : ScriptableObject
    {
        public string Name;

        [HideInInspector][SerializeField] List<NodeAsset> handlers;

        public List<NodeAsset> Handlers
        {
            get => handlers;
        }
    }
}

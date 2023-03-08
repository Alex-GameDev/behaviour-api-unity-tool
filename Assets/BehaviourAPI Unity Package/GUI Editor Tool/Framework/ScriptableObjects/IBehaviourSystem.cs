using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public interface IBehaviourSystem
    {
        public Object ObjectReference { get; }
        public ScriptableObject AssetReference { get; }
        public Component ComponentReference { get; }


        public List<GraphAsset> Graphs { get; }
        public GraphAsset MainGraph { get; set; }
        public List<PerceptionAsset> PullPerceptions { get; }
        public List<PushPerceptionAsset> PushPerceptions { get; }
    }
}
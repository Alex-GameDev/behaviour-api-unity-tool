using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public interface IBehaviourSystem
    {
        public List<GraphAsset> Graphs { get; }
        public GraphAsset MainGraph { get; set; }
        public List<PerceptionAsset> PullPerceptions { get; }
        public List<PushPerceptionAsset> PushPerceptions { get; }

        public GraphAsset CreateGraph(string name, Type graphType);
        public void RemoveGraph(GraphAsset graphAsset);

        public PerceptionAsset CreatePerception(string name, Type type);
        public void RemovePerception(PerceptionAsset perception);

        public PushPerceptionAsset CreatePushPerception(string name);
        public void RemovePushPerception(PushPerceptionAsset pushPerception);


        public void OnSubAssetCreated(ScriptableObject asset);
        public void OnSubAssetRemoved(ScriptableObject asset);
        public void OnModifyAsset();
        public void Save();
    }
}
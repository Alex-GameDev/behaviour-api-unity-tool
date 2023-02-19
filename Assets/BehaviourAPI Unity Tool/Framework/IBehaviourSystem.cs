using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public interface IBehaviourSystem
    {
        public List<GraphAsset> Graphs { get; }
        public GraphAsset MainGraph { get; set; }

        public GraphAsset CreateGraph(string name, Type graphType);
        public void RemoveGraph(GraphAsset graphAsset);

        public void OnSubAssetCreated(ScriptableObject asset);
        public void OnSubAssetRemoved(ScriptableObject asset);
        public void OnModifyAsset();
        public void Save();
    }
}
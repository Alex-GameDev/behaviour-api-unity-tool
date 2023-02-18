using System;
using System.Collections.Generic;


namespace BehaviourAPI.Unity.Framework
{
    public interface IBehaviourSystem
    {
        public List<GraphAsset> Graphs { get; }
        public GraphAsset MainGraph { get; set; }

        public GraphAsset CreateGraph(string name, Type graphType);

        public void RemoveGraph(GraphAsset graphAsset);

        public void Save();
    }
}
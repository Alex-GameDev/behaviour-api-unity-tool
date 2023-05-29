using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System.Collections.Generic;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    public class BuildedSystemData
    {
        public BehaviourGraph MainGraph { get; private set; }
        public Dictionary<string, BehaviourGraph> GraphMap { get; private set; }
        public Dictionary<string, PushPerception> PushPerceptionMap { get; private set; }

        public BuildedSystemData(BehaviourGraph mainGraph, Dictionary<string, BehaviourGraph> graphMap, Dictionary<string, PushPerception> pushPerceptionMap)
        {
            MainGraph = mainGraph;
            GraphMap = graphMap;
            PushPerceptionMap = pushPerceptionMap;
        }
    }
}

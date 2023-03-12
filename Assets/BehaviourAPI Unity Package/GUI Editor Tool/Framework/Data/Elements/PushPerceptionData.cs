using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [Serializable]
    public class PushPerceptionData : ICloneable, IBuildable
    {
        public string name;
        [HideInInspector] public PushPerception pushPerception;
        [HideInInspector] public List<string> targetNodeIds;

        public void Build(SystemData data)
        {
            pushPerception = new PushPerception();

            // TODO: Add listeners
        }

        public object Clone()
        {
            PushPerceptionData copy = new PushPerceptionData();
            copy.name = name;
            copy.targetNodeIds = new List<string>(targetNodeIds);
            return copy;
        }
    }
}

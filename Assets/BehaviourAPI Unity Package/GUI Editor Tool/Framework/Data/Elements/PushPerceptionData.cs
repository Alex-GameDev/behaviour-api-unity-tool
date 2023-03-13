using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    [Serializable]
    public class PushPerceptionData : ICloneable, IBuildable
    {
        public string name;
        [HideInInspector] public PushPerception pushPerception;
        [HideInInspector] public List<string> targetNodeIds = new List<string>();

        public PushPerceptionData()
        {
        }

        public PushPerceptionData(string name)
        {
            this.name = name;
        }

        public void Build(SystemData data)
        {
            pushPerception = new PushPerception();

            if(targetNodeIds.Count > 0)
            {
                var allNodes = data.graphs.SelectMany(g => g.nodes).ToList();
                for (int i = 0; i < targetNodeIds.Count; i++)
                {
                    var node = allNodes.Find(node => node.id == targetNodeIds[i]);
                    var pushTarget = node?.node as IPushActivable;
                    pushPerception.PushListeners.Add(pushTarget);
                }
            }
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

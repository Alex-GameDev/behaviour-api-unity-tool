
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    using Core.Perceptions;

    /// <summary>
    /// Clas that serializes a push perception
    /// </summary>
    [Serializable]
    public class PushPerceptionData : ICloneable, IBuildable
    {
        /// <summary>
        /// The name of the push perception.
        /// </summary>
        public string name;

        /// <summary>
        /// The push perception stored.
        /// </summary>
        [HideInInspector] public PushPerception pushPerception;

        /// <summary>
        /// The id of the target nodes in the system data.
        /// </summary>
        [HideInInspector] public List<string> targetNodeIds = new List<string>();

        /// <summary>
        /// Default construction
        /// </summary>
        public PushPerceptionData()
        {
        }

        /// <summary>
        /// Create a new 
        /// </summary>
        /// <param name="name"></param>
        public PushPerceptionData(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// <inheritdoc/>
        /// Set the <see cref="pushPerception"/> target nodes searching the nodes in system data by id.
        /// </summary>
        /// <param name="data"><inheritdoc/></param>
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

        /// <summary>
        /// Create a copy of the push perception data. 
        /// Used to create a runtime copy.
        /// </summary>
        /// <returns>A deep copy of the data.</returns>
        public object Clone()
        {
            PushPerceptionData copy = new PushPerceptionData();
            copy.name = name;
            copy.targetNodeIds = new List<string>(targetNodeIds);
            return copy;
        }
    }
}

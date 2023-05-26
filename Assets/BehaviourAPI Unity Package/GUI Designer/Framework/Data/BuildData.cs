using BehaviourAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Framework
{
    public class BuildData
    {
        /// <summary>
        /// Used to generate the reflected methods.
        /// </summary>
        public Component Runner { get; private set; }

        /// <summary>
        /// Used to find the behaviour engine references.
        /// </summary>
        public Dictionary<string, BehaviourGraph> GraphMap { get; private set; }

        /// <summary>
        /// Used to find the node references.
        /// </summary>
        public Dictionary<string, Node> NodeMap { get; private set; }

        /// <summary>
        /// Create a new build data.
        /// </summary>
        /// <param name="component">The component used to find the behaviour engine references.</param>
        /// <param name="systemData">The behaviour system used to create the graph and node maps.</param>
        public BuildData(Component component, SystemData systemData)
        {
            this.Runner = component;

            GraphMap = systemData.graphs.ToDictionary(g => g.id, g => g.graph);
            NodeMap = systemData.graphs.SelectMany(g => g.nodes).ToDictionary(n => n.id, n => n.node);
        }
    }
}

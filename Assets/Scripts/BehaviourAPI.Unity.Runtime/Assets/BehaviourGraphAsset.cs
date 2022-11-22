using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    [CreateAssetMenu(menuName = "BehaviourAPI/Graph", order = 0)]
    public class BehaviourGraphAsset : ScriptableObject
    {
        /// <summary>
        /// The behaviour graph
        /// </summary>
        [SerializeReference] BehaviourGraph graph;

        /// <summary>
        /// The elements that contains this Graph
        /// </summary>
        [SerializeReference] List<EnterSystemAction> Containers; // TODO: Cambiar por IGraphContainer

        /// <summary>
        /// The node assets in this graph
        /// </summary>
        public List<NodeAsset> Nodes;

        /// <summary>
        /// The connection assets in this graph
        /// </summary>
        public List<ConnectionAsset> Connections;

        /// <summary>
        /// Build the graph internal references
        /// </summary>
        public void BuildGraph()
        {
            Nodes.ForEach(node => node.AssembleConnections());
            // graph.Build();
        }
    }


}
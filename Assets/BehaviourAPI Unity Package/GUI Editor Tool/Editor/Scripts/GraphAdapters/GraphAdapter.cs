using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Editor
{
    /// <summary>
    /// Allows editor tools to use specific graph type.
    /// </summary>
    public abstract class GraphAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        public EditorHierarchyNode NodeHierarchy { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphType"></param>
        /// <param name="nodeTypes"></param>
        public void BuildSupportedHerarchy(Type graphType, List<Type> nodeTypes)
        {
            var validTypes = GetValidNodeTypes(graphType, nodeTypes);
            NodeHierarchy = CreateNodeHierarchy(graphType, validTypes);
        }

        public abstract void AutoLayout(GraphData graphData);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphtype"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        protected abstract EditorHierarchyNode CreateNodeHierarchy(Type graphtype, List<Type> types);

        #region ----------------- Static methods ----------------


        private static Dictionary<Type, GraphAdapter> factoryCache = new Dictionary<Type, GraphAdapter>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graphType"></param>
        /// <returns></returns>
        public static GraphAdapter GetAdapter(Type graphType)
        {
            var metadata = BehaviourAPISettings.instance.Metadata;
            if (metadata.GraphAdapterMap.TryGetValue(graphType, out Type adapterType))
            {
                return GetOrCreate(adapterType, graphType);
            }
            else
            {
                return null;
            }
        }

        private static GraphAdapter GetOrCreate(Type adapterType, Type graphType)
        {
            if (factoryCache.TryGetValue(adapterType, out GraphAdapter adapter))
            {
                return adapter;
            }
            else
            {
                adapter = (GraphAdapter)Activator.CreateInstance(adapterType);
                adapter.BuildSupportedHerarchy(graphType, BehaviourAPISettings.instance.Metadata.NodeTypes);
                factoryCache[adapterType] = adapter;
                return adapter;
            }
        }

        static List<Type> GetValidNodeTypes(Type graphType, List<Type> nodeTypes)
        {
            var graph = (BehaviourGraph)Activator.CreateInstance(graphType);
            List<Type> validNodeTypes = new List<Type>();
            for (int i = 0; i < nodeTypes.Count; i++)
            {
                var node = (Node)Activator.CreateInstance(nodeTypes[i]);
                if(node.GraphType.IsAssignableFrom(graphType) && graph.NodeType.IsAssignableFrom(nodeTypes[i]))
                {
                    validNodeTypes.Add(nodeTypes[i]);
                }

            }
            return validNodeTypes;
        }

        #endregion
    }
}

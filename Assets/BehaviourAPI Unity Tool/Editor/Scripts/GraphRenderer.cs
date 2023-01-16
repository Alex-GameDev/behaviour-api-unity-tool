using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using log4net.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class GraphRenderer
    {
        protected Dictionary<NodeAsset, NodeView> assetViewPairs = new Dictionary<NodeAsset, NodeView>();
        public BehaviourGraphView graphView;

        /// <summary>
        /// Draws a node in the graphView
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public abstract NodeView DrawNode(NodeAsset asset);

        /// <summary>
        /// Draws a edge between two nodes
        /// </summary>
        /// <param name="src"></param>
        /// <param name="tgt"></param>
        /// <returns></returns>
        public abstract void DrawConnections(NodeAsset asset);

        /// <summary>
        /// Get compatible ports
        /// </summary>
        /// <param name="ports"></param>
        /// <param name="startPort"></param>
        /// <returns></returns>
        public abstract List<Port> GetValidPorts(UQueryState<Port> ports, Port startPort);

        /// <summary>
        /// Get the hierarchy entries to the node create search window.
        /// </summary>
        /// <returns></returns>
        public abstract List<SearchTreeEntry> GetNodeHierarchyEntries();

        // (!) Ejecutar después de haber borrado los nodos del grafo
        public abstract GraphViewChange OnGraphViewChanged(GraphViewChange change);

        /// <summary>
        /// Find a renderer
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static GraphRenderer FindRenderer(BehaviourGraph graph)
        {
            var types = BehaviourAPISettings.instance.GetTypes().FindAll(t => 
                t.IsSubclassOf(typeof(GraphRenderer)) &&
                t.GetCustomAttributes().Any(a => a is CustomRendererAttribute crAttrib && crAttrib.type == graph.GetType()));

            if(types.Count() > 0) return Activator.CreateInstance(types[0]) as GraphRenderer;
            else return null;
        }

        public abstract void DrawGraph(GraphAsset graphAsset);

        protected SearchTreeEntry GetTypeEntry(Type type, int level)
        {
            return new SearchTreeEntry(new GUIContent("     " + Regex.Replace(type.Name, "([A-Z])", " $1").Trim())) 
            { 
                userData = type, level = level 
            };
        }
    }
}

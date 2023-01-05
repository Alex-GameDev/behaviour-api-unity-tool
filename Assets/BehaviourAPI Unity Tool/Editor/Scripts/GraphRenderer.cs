using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourAPI.Unity.Editor
{
    public abstract class GraphRenderer
    {
        protected Dictionary<NodeAsset, NodeView> assetViewPairs;

        /// <summary>
        /// Defines actions for the node contextual menu
        /// </summary>
        /// <param name="nodeView"></param>
        /// <param name="menuEvt"></param>
        public abstract void BuildContextualMenu(NodeView nodeView, ContextualMenuPopulateEvent menuEvt);

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
        public abstract Edge DrawEdge(NodeAsset src, NodeAsset tgt);

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
    }
}

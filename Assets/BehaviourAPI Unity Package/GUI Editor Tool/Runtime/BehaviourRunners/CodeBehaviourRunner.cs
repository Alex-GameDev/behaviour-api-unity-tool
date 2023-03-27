using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Subclass of <see cref="BehaviourRunner"/> that creates the behaviour system in code.
    /// </summary>
    public abstract class CodeBehaviourRunner : BehaviourRunner
    {
        #region -------------------------------- private fields ---------------------------------

        readonly Dictionary<BehaviourGraph, string> allgraphs = new Dictionary<BehaviourGraph, string>();

        #endregion

        #region ------------------------------ Execution Methods ------------------------------

        public sealed override SystemData GetBehaviourSystemAsset()
        {
            return new SystemData(allgraphs);
        }

        protected sealed override BehaviourGraph GetExecutionGraph() => CreateGraph();

        /// <summary>
        /// Register a graph to use it in a <see cref="BSRuntimeDebugger"/>.
        /// </summary>
        /// <param name="graph">The graph registered.</param>
        /// <param name="name">The name of the graph in the debugger window.</param>
        public void RegisterGraph(BehaviourGraph graph, string name = "")
        {
            allgraphs.Add(graph, name);
        }

        /// <summary>
        /// Create the behaviour graph(s).
        /// </summary>
        /// <returns>The main <see cref="BehaviourGraph"/>. </returns>
        protected abstract BehaviourGraph CreateGraph();

        #endregion
    }
}

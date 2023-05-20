using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    using Core;
    using Core.Perceptions;
    using Unity.Framework;

    /// <summary>
    /// Base class of components that use an editable behavior system.
    /// </summary>
    public abstract class DataBehaviourRunner : BehaviourRunner
    {
        #region -------------------------------- private fields ---------------------------------

        SystemData _executionSystem;

        Dictionary<string, BehaviourGraph> _graphMap;
        Dictionary<string, PushPerception> _pushPerceptionMap;

        #endregion

        #region --------------------------------- properties ----------------------------------

        /// <summary>
        /// The execution main graph
        /// </summary>
        public BehaviourGraph BuildedGraph { get; private set; }

        #endregion

        #region ------------------------------ Execution Methods ------------------------------

        protected sealed override BehaviourGraph GetExecutionGraph()
        {
            _executionSystem = GetEditorSystemData();
            _executionSystem.BuildSystem();

            BuildDictionaries();
            BuildedGraph = _executionSystem.graphs[0].graph;

            ModifyGraphs();
            return BuildedGraph;
        }

        /// <summary>
        /// Overrides this method to modify the created system in code just after build it.
        /// </summary>
        protected virtual void ModifyGraphs() { }

        /// <summary>
        /// Get the system created in editor mode to build the runtime one.
        /// </summary>
        protected abstract SystemData GetEditorSystemData();

        #endregion

        #region ------------------------------------ Find elements ---------------------------------------

        /// <summary>
        /// Build the internal dictionaries to search elements runtime.
        /// </summary>
        void BuildDictionaries()
        {
            _graphMap = new Dictionary<string, BehaviourGraph>();
            foreach (GraphData data in _executionSystem.graphs)
            {
                if (!string.IsNullOrWhiteSpace(data.name))
                {
                    if (!_graphMap.TryAdd(data.name, data.graph))
                    {
                        Debug.LogWarning($"ERROR: Graph \"{data.name}\" wasn't added to dictionary because a graph with the same name was added before.", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"ERROR: Graph \"{data.name}\" wasn't added to dictionary because the name is not valid", this);
                }
            }

            _pushPerceptionMap = new Dictionary<string, PushPerception>();
            foreach (PushPerceptionData data in _executionSystem.pushPerceptions)
            {
                if (!string.IsNullOrWhiteSpace(data.name))
                {
                    if (!_pushPerceptionMap.TryAdd(data.name, data.pushPerception))
                    {
                        Debug.LogWarning($"ERROR: Push perception \"{data.name}\" wasn't added to dictionary because a push perception with the same name was added before.", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"ERROR: Push perception \"{data.name}\" wasn't added to dictionary because the name is not valid", this);
                }
            }
        }

        /// <summary>
        /// Find a push perception by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the push perception</param>
        /// <returns>The <see cref="PushPerception"></see> found.</returns>
        public PushPerception FindPushPerception(string name)
        {
            return _pushPerceptionMap[name];
        }

        /// <summary>
        /// Find a push perception by its name or null if doesn't exists.
        /// </summary>
        /// <param name="name">The name of the push perception</param>
        /// <returns>The <see cref="PushPerception"></see> found.</returns>
        public PushPerception FindPushPerceptionOrDefault(string name)
        {
            return _pushPerceptionMap.GetValueOrDefault(name);
        }

        /// <summary>
        /// Find a graph by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public BehaviourGraph FindGraph(string name)
        {
            return _graphMap[name];
        }

        /// <summary>
        /// Find a graph by its name or null if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public BehaviourGraph FindGraphOrDefault(string name)
        {
            return _graphMap.GetValueOrDefault(name);
        }

        /// <summary>
        /// Find a graph of type <typeparamref name="T"/> by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the graph.</typeparam>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public T FindGraph<T>(string name) where T : BehaviourGraph
        {
            if (_graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
                else throw new InvalidCastException($"Graph \"{name}\" exists, but is not an instance of {typeof(T).FullName} class.");
            }
            else
            {
                throw new KeyNotFoundException($"Graph \"{name}\" doesn't exist.");
            }
        }

        /// <summary>
        /// Find a graph of type <typeparamref name="T"/> by its name or null if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the graph.</typeparam>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public T FindGraphOrDefault<T>(string name) where T : BehaviourGraph
        {
            if (_graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
            }
            return null;
        }


        #endregion
    }
}

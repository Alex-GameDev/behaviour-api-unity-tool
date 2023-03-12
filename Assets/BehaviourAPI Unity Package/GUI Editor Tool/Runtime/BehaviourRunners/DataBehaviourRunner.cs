using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class DataBehaviourRunner : BehaviourRunner
    {
        #region -------------------------------- private fields ---------------------------------

        [Tooltip("Defines how the graphs and nodes are saved to be searched after.")]
        [SerializeField] NamingSettings graphNamingSettings = NamingSettings.IgnoreWhenInvalid;

        [Tooltip("Defines how the perceptions are saved to be searched after.")]
        [SerializeField] NamingSettings perceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        [Tooltip("Defines how the push perceptions are saved to be searched after.")]
        [SerializeField] NamingSettings pushPerceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        BehaviourSystem _executionSystem;

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
            var system = GetEditorSystem();
            var duplicator = new Duplicator();
            _executionSystem = duplicator.Duplicate(system);

            _executionSystem.Build(graphNamingSettings, perceptionNamingSettings, pushPerceptionNamingSettings);
            BuildedGraph = _executionSystem.MainGraph.Graph;

            ModifyGraphs();
            return BuildedGraph;
        }

        public sealed override BehaviourSystem GetBehaviourSystemAsset()
        {
            return _executionSystem;
        }

        /// <summary>
        /// Overrides this method to modify the created system in code just after build it.
        /// </summary>
        protected virtual void ModifyGraphs() { }

        /// <summary>
        /// Get the system created in editor mode to build the runtime one.
        /// </summary>
        protected abstract BehaviourSystem GetEditorSystem();

        #endregion

        #region ------------------------------------ Find elements ---------------------------------------

        /// <summary>
        /// Find a push perception by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the push perception</param>
        /// <returns>The <see cref="PushPerception"></see> found.</returns>
        public PushPerception FindPushPerception(string name)
        {
            return _executionSystem.pushPerceptionMap[name];
        }

        /// <summary>
        /// Find a push perception by its name or null if doesn't exists.
        /// </summary>
        /// <param name="name">The name of the push perception</param>
        /// <returns>The <see cref="PushPerception"></see> found.</returns>
        public PushPerception FindPushPerceptionOrDefault(string name)
        {
            return _executionSystem.pushPerceptionMap.GetValueOrDefault(name);
        }

        /// <summary>
        /// Find a perception by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the perception</param>
        /// <returns>The <see cref="Perception"></see> found.</returns>
        public Perception FindPerception(string name)
        {
            return _executionSystem.pullPerceptionMap[name];
        }

        /// <summary>
        /// Find a push perception by its name or null if doesn't exists.
        /// </summary>
        /// <param name="name">The name of the push perception</param>
        /// <returns>The <see cref="PushPerception"></see> found./></returns>
        public Perception FindPerceptionOrDefault(string name)
        {
            return _executionSystem.pullPerceptionMap.GetValueOrDefault(name);
        }

        /// <summary>
        /// Find a perception of type <typeparamref name="T"/> by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the perception.</typeparam>
        /// <param name="name">The name of the perception</param>
        /// <returns>The <see cref="Perception"></see> found.</returns>
        public T FindPerception<T>(string name) where T : Perception
        {
            if (_executionSystem.pullPerceptionMap.TryGetValue(name, out var perception))
            {
                if (perception is T perceptionTyped) return perceptionTyped;
                else throw new InvalidCastException($"Perception \"{name}\" exists, but is not an instance of {typeof(T).FullName} class.");
            }
            else
            {
                throw new KeyNotFoundException($"Perception \"{name}\" doesn't exist.");
            }
        }

        /// <summary>
        /// Find a perception of type <typeparamref name="T"/> by its name or null if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the perception.</typeparam>
        /// <param name="name">The name of the perception</param>
        /// <returns>The <see cref="Perception"></see> found.</returns>
        public T FindPerceptionOrDefault<T>(string name) where T : Perception
        {
            if (_executionSystem.pullPerceptionMap.TryGetValue(name, out var perception))
            {
                if (perception is T perceptionTyped) return perceptionTyped;
            }
            return null;
        }

        /// <summary>
        /// Find a graph by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public BehaviourGraph FindGraph(string name)
        {
            return _executionSystem.graphMap[name];
        }

        /// <summary>
        /// Find a graph by its name or null if doesn't exist.
        /// </summary>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public BehaviourGraph FindGraphOrDefault(string name)
        {
            return _executionSystem.graphMap.GetValueOrDefault(name);
        }

        /// <summary>
        /// Find a graph of type <typeparamref name="T"/> by its name. Throws and exception if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The type of the graph.</typeparam>
        /// <param name="name">The name of the graph</param>
        /// <returns>The <see cref="BehaviourGraph"></see> found.</returns>
        public T FindGraph<T>(string name) where T : BehaviourGraph
        {
            if (_executionSystem.graphMap.TryGetValue(name, out var graph))
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
            if (_executionSystem.graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
            }
            return null;
        }


        #endregion
    }
}

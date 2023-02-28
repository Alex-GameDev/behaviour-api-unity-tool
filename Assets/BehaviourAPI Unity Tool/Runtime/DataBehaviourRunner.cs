using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class DataBehaviourRunner : BehaviourRunner
    {
        public NamingSettings nodeNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings perceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings pushPerceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        /// <summary>
        /// The runtime copy of the behaviour system
        /// </summary>
        public BehaviourSystemAsset ExecutionSystem { get; private set; }

        public BehaviourGraph BuildedGraph { get; private set; }

        /// <summary>
        /// Generates a runtime copy of the editor system
        /// </summary>
        protected override BehaviourGraph GetExecutionGraph()
        {
            var system = GetEditorSystem();
            var duplicator = new Duplicator();
            ExecutionSystem = duplicator.Duplicate(system);

            ExecutionSystem.Build(nodeNamingSettings, perceptionNamingSettings, perceptionNamingSettings);
            BuildedGraph = ExecutionSystem.MainGraph.Graph;

            ModifyGraphs();
            return BuildedGraph;
        }

        /// <summary>
        /// Overrides this method to modify the created system in code
        /// </summary>
        protected virtual void ModifyGraphs() { }

        /// <summary>
        /// Get the system created in editor mode
        /// </summary>
        protected abstract BehaviourSystemAsset GetEditorSystem();

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return ExecutionSystem;
        }
      

        #region ------------------------------------ Find elements ---------------------------------------

        public PushPerception FindPushPerception(string name)
        {
            return ExecutionSystem.pushPerceptionMap[name];
        }

        public PushPerception FindPushPerceptionOrDefault(string name)
        {
            return ExecutionSystem.pushPerceptionMap.GetValueOrDefault(name);
        }

        public Perception FindPerception(string name)
        {
            return ExecutionSystem.pullPerceptionMap[name];
        }

        public Perception FindPerceptionOrDefault(string name)
        {
            return ExecutionSystem.pullPerceptionMap.GetValueOrDefault(name);
        }

        public T FindPerception<T>(string name) where T : Perception
        {
            if (ExecutionSystem.pullPerceptionMap.TryGetValue(name, out var perception))
            {
                if (perception is T perceptionTyped) return perceptionTyped;
                else throw new InvalidCastException($"Perception \"{name}\" exists, but is not an instance of {typeof(T).FullName} class.");
            }
            else
            {
                throw new KeyNotFoundException($"Perception \"{name}\" doesn't exist.");
            }
        }

        public T FindPerceptionOrDefault<T>(string name) where T : Perception
        {
            if (ExecutionSystem.pullPerceptionMap.TryGetValue(name, out var perception))
            {
                if (perception is T perceptionTyped) return perceptionTyped;
            }
            return null;
        }

        public BehaviourGraph FindGraph(string name)
        {
            return ExecutionSystem.graphMap[name];
        }

        public BehaviourGraph FindGraphOrDefault(string name)
        {
            return ExecutionSystem.graphMap.GetValueOrDefault(name);
        }

        public T FindGraph<T>(string name) where T : BehaviourGraph
        {
            if (ExecutionSystem.graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
                else throw new InvalidCastException($"Graph \"{name}\" exists, but is not an instance of {typeof(T).FullName} class.");
            }
            else
            {
                throw new KeyNotFoundException($"Graph \"{name}\" doesn't exist.");
            }
        }

        public T FindGraphOrDefault<T>(string name) where T : BehaviourGraph
        {
            if (ExecutionSystem.graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
            }
            return null;
        }


        #endregion
    }
}

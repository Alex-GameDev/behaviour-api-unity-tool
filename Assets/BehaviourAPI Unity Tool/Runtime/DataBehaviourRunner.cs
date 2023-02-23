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

        Dictionary<string, PushPerception> pushPerceptionMap;
        Dictionary<string, Perception> pullPerceptionMap;
        Dictionary<string, BehaviourGraph> graphMap;

        protected BehaviourGraph _buildedMainGraph;

        /// <summary>
        /// The runtime copy of the behaviour system
        /// </summary>
        public BehaviourSystemAsset ExecutionSystem { get; private set; }

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return ExecutionSystem;
        }

        protected override void OnAwake()
        {
            var system = GetSystem();
            var duplicator = new Duplicator();
            ExecutionSystem = duplicator.Duplicate(system);

            ExecutionSystem.Build(nodeNamingSettings, perceptionNamingSettings, perceptionNamingSettings);
            _buildedMainGraph = ExecutionSystem.MainGraph.Graph;
            _buildedMainGraph.SetExecutionContext(new UnityExecutionContext(gameObject));
        }

        protected abstract BehaviourSystemAsset GetSystem();

        protected override void OnStart()
        {
            if (_buildedMainGraph != null)
            {
                Debug.Log("Start graph");
                _buildedMainGraph.Start();
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        protected override void OnUpdate()
        {
            if (_buildedMainGraph != null)
            {
                _buildedMainGraph.Update();
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        private void OnDisable()
        {
            _buildedMainGraph?.Stop();
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

        public Perception FindPerception<T>(string name) where T : Perception
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

        public Perception FindPerceptionOrDefault<T>(string name) where T : Perception
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

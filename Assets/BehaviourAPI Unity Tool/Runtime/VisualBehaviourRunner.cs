using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class VisualBehaviourRunner : BehaviourRunner
    {
        public NamingSettings nodeNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings perceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;
        public NamingSettings pullPerceptionNamingSettings = NamingSettings.IgnoreWhenInvalid;

        [HideInInspector] public BehaviourSystemAsset SystemAsset;

        BehaviourGraph _rootGraph;

        protected override void OnAwake()
        {
            if(SystemAsset == null )
            {
                Debug.LogError("Not behaviour system attached. Component is removed.");
                Destroy(this);
            }
            else
            {
                SystemAsset.Build(nodeNamingSettings, perceptionNamingSettings, pullPerceptionNamingSettings);

                if (_rootGraph == null)
                {
                    Debug.LogError("Behaviour system is empty.", this);                    
                }
            }
        }

        protected override void OnStart()
        {
            _rootGraph.Start();
        }

        protected override void OnUpdate()
        {
            _rootGraph.Update();
        }

        public override BehaviourSystemAsset GetBehaviourSystemAsset()
        {
            return SystemAsset;
        }

        #region ------------------------------------ Find elements ---------------------------------------

        public PushPerception FindPushPerception(string name)
        {
            return SystemAsset.pushPerceptionMap[name];
        }

        public PushPerception FindPushPerceptionOrDefault(string name)
        {
            return SystemAsset.pushPerceptionMap.GetValueOrDefault(name);
        }

        public Perception FindPerception(string name)
        {
            return SystemAsset.pullPerceptionMap[name];
        }

        public Perception FindPerceptionOrDefault(string name)
        {
            return SystemAsset.pullPerceptionMap.GetValueOrDefault(name);
        }

        public Perception FindPerception<T>(string name) where T : Perception
        {
            if (SystemAsset.pullPerceptionMap.TryGetValue(name, out var perception))
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
            if (SystemAsset.pullPerceptionMap.TryGetValue(name, out var perception))
            {
                if (perception is T perceptionTyped) return perceptionTyped;
            }
            return null;
        }

        public BehaviourGraph FindGraph(string name)
        {
            return SystemAsset.graphMap[name];
        }

        public BehaviourGraph FindGraphOrDefault(string name)
        {
            return SystemAsset.graphMap.GetValueOrDefault(name);
        }

        public T FindGraph<T>(string name) where T : BehaviourGraph
        {
            if (SystemAsset.graphMap.TryGetValue(name, out var graph))
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
            if (SystemAsset.graphMap.TryGetValue(name, out var graph))
            {
                if (graph is T graphTyped) return graphTyped;
            }
            return null;
        }


        #endregion
    }
}

using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class VisualBehaviourRunner : BehaviourRunner
    {
        [HideInInspector] public BehaviourSystemAsset SystemAsset;

        BehaviourGraph _rootGraph;

        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        protected override void OnAwake()
        {
            if(SystemAsset == null )
            {
                Debug.LogError("Not behaviour system attached. Component is removed.");
                Destroy(this);
            }
            else
            {
                _rootGraph = SystemAsset.Build();

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

        /// <summary>
        /// Find a push perception by its name
        /// </summary>
        /// <param name="name">The name of the push perception asset</param>
        /// <returns>The push perception found, or null.</returns>
        public PushPerception FindPushPerception(string name)
        {
            return SystemAsset.FindPushPerception(name);
        }

        /// <summary>
        /// Find a perception by its name
        /// </summary>
        /// <param name="name">The name of the perception asset</param>
        /// <returns>The perception found or null.</returns>
        public Perception FindPerception(string name)
        {
            return SystemAsset.FindPerception(name);
        }

        public T FindNode<T>(string nodeName, string graphName = null) where T : Node
        {
            return SystemAsset.FindNode<T>(nodeName, graphName);
        }
    }
}

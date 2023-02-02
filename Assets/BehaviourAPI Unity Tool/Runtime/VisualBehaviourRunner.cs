using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using Codice.CM.Common.Replication;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
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
                    Debug.LogError("Behaviour system is empty.");                    
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

        protected PushPerception FindPushPerception(string name)
        {
            return SystemAsset.GetPushPerception(name);
        }
    }
}

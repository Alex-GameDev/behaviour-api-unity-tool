using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class BehaviourGraphVisualRunner : MonoBehaviour
    {
        public BehaviourSystemAsset SystemAsset;

        BehaviourGraph _rootGraph;

        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        protected virtual void OnAwake()
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
                    Debug.LogError("Behaviour system is empty. Component is removed.");
                    Destroy(this);
                }
            }
        }

        protected virtual void OnStart()
        {
            _rootGraph.Start();
        }

        protected virtual void OnUpdate()
        {
            _rootGraph.Update();
        }

        public Status Test()
        {
            Debug.Log("Trying custom action");
            return Status.Success;
        }

        public PushPerception FindPerception(string name)
        {
            return SystemAsset.GetPushPerception(name);
        }
    }
}

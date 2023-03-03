using UnityEngine;
using BehaviourAPI.Unity.Framework;
using System;
using BehaviourAPI.Core;
using BehaviourAPI.UnityExtensions;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class BehaviourRunner : MonoBehaviour
    {
        public bool ExecuteOnLoop;
        public bool DontStopOnDisable;

        bool _systemRunning;

        BehaviourGraph _executionGraph;

        public abstract BehaviourSystemAsset GetBehaviourSystemAsset();

        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        private void OnEnable() => OnEnableSystem();

        private void OnDisable() => OnDisableSystem();

        protected virtual void OnAwake()
        {
            _executionGraph = GetExecutionGraph();

            if(_executionGraph != null)
            {
                UnityExecutionContext context = new UnityExecutionContext(gameObject);
                _executionGraph.SetExecutionContext(context);
            }
        }

        protected abstract BehaviourGraph GetExecutionGraph();

        protected virtual void OnStart()
        {
            if(_executionGraph != null)
            {
                _executionGraph.Start();
                _systemRunning = true;
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        protected virtual void OnUpdate()
        {
            if (_executionGraph != null)
            {
                _executionGraph.Update();

                if(ExecuteOnLoop && _executionGraph.Status != Status.Running)
                {
                    _executionGraph.Restart();
                }
            }
            else
            {
                Debug.LogWarning("[BehaviourRunner] - This runner has not graphs attached.", this);
                Destroy(this);
            }
        }

        protected virtual void OnEnableSystem()
        {
            if(!DontStopOnDisable && _systemRunning)
            {
                if(_executionGraph != null)
                {
                    _executionGraph.Start();
                }
            }
        }
        protected virtual void OnDisableSystem()
        {
            if (!DontStopOnDisable && _systemRunning)
            {
                if (_executionGraph != null)
                {
                    _executionGraph.Stop();
                }
            }
        }
    }
}

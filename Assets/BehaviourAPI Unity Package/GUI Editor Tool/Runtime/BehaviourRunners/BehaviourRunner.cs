using UnityEngine;
using BehaviourAPI.Unity.Framework;
using System;
using BehaviourAPI.Core;
using BehaviourAPI.UnityExtensions;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Base class for behaviour system runners
    /// </summary>
    public abstract class BehaviourRunner : MonoBehaviour
    {
        #region -------------------------------- public variables --------------------------------

        /// <summary>
        /// True if the main graph should be restarted on update when it finished with Success or Failure
        /// </summary>
        public bool ExecuteOnLoop;

        /// <summary>
        /// True if the main graph souldn't be restarted when the component is disabled and enable again
        /// </summary>
        public bool DontStopOnDisable;

        #endregion

        #region -------------------------------- private fields ---------------------------------

        bool _systemRunning;

        BehaviourGraph _executionGraph;

        #endregion

        #region --------------------------------- Unity Events ----------------------------------

        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        private void OnEnable() => OnEnableSystem();

        private void OnDisable() => OnDisableSystem();

        #endregion

        #region ----------------------------- Protected methods -----------------------------

        /// <summary>
        /// Override this method to modify the Awake event instead of implement Awake directly.
        /// </summary>
        protected virtual void OnAwake()
        {
            _executionGraph = GetExecutionGraph();

            if(_executionGraph != null)
            {
                UnityExecutionContext context = new UnityExecutionContext(gameObject);
                _executionGraph.SetExecutionContext(context);
            }
        }

        /// <summary>
        /// Override this method to modify the Start event instead of implement Start directly.
        /// </summary>
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

        /// <summary>
        /// Override this method to modify the Update event instead of implement Update directly.
        /// </summary>
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

        /// <summary>
        /// Override this method to modify the OnEnable event instead of implement OnEnable directly.
        /// </summary>
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

        /// <summary>
        /// Override this method to modify the OnDisable event instead of implement OnDisable directly.
        /// </summary>
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

        /// <summary>
        /// Returns the system that can be used by the <see cref="BSRuntimeDebugger"/> component.
        /// </summary>
        /// <returns>The <see cref="SystemData"/> debuggable.</returns>
        public abstract SystemData GetBehaviourSystemAsset();

        /// <summary>
        /// Gets the main graph that will be executed.
        /// </summary>
        /// <returns>The execution <see cref="BehaviourGraph"></see></returns>
        protected abstract BehaviourGraph GetExecutionGraph();

        #endregion
    }
}

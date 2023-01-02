using BehaviourAPI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Component used for display the status of
    /// </summary>
    
    [RequireComponent(typeof(BehaviourGraphVisualRunner))]
    public class BSRuntimeDebugger : MonoBehaviour
    {
        BehaviourGraphVisualRunner _runner;

        [SerializeField] bool _logGraphStatusChanged;
        [SerializeField] bool _logNodeStatusChanged;
        [SerializeField] bool _openDebuggerOnPlay;

        void Awake()
        {
            _runner = GetComponent<BehaviourGraphVisualRunner>();
        }

        private void Start()
        {
            _runner.SystemAsset.RootGraph.Graph.StatusChanged += RootGraphStatusChanged;
        }

        void RootGraphStatusChanged(Status status)
        {
            Debug.Log(status);
        }
    }
}

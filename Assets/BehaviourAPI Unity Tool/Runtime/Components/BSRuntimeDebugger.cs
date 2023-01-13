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
    
    [RequireComponent(typeof(VisualBehaviourRunner))]
    public class BSRuntimeDebugger : MonoBehaviour
    {
        public BehaviourSystemAsset systemAsset;


        [SerializeField] bool _logGraphStatusChanged;
        [SerializeField] bool _logNodeStatusChanged;
        [SerializeField] bool _openDebuggerOnPlay;

        void Awake()
        {
            systemAsset = GetComponent<BehaviourRunner>().GetBehaviourSystemAsset();
        }
    }
}

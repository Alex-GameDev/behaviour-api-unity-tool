using BehaviourAPI.Unity.Framework;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Component used for display the status of
    /// </summary>
    
    [RequireComponent(typeof(BehaviourRunner))]
    public class BSRuntimeDebugger : MonoBehaviour
    {
        [HideInInspector] public BehaviourSystemAsset systemAsset;

        [SerializeField] bool _logGraphStatusChanged;
        [SerializeField] bool _logNodeStatusChanged;
        [SerializeField] bool _openDebuggerOnPlay;

        public bool IsDebuggerReady { get; private set; } = false;

        void Start()
        {
            systemAsset = GetComponent<BehaviourRunner>().GetBehaviourSystemAsset();
            IsDebuggerReady = true;
        }
    }
}

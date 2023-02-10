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

        public bool IsDebuggerReady { get; private set; } = false;

        void Start()
        {
            systemAsset = GetComponent<BehaviourRunner>().GetBehaviourSystemAsset();
            IsDebuggerReady = true;
        }
    }
}

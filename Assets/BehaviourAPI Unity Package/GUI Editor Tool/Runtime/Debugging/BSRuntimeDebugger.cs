using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    using UnityExtensions;
 
    [RequireComponent(typeof(BehaviourRunner))]
    [DefaultExecutionOrder(1000)]

    /// <summary>
    /// Component used for display the status of
    /// </summary>   
    public class BSRuntimeDebugger : MonoBehaviour
    {
        [HideInInspector] public BehaviourSystem systemAsset;

        [SerializeField] bool debugStatusChanges;

        Dictionary<IStatusHandler, Status> _lastFinishedStatusMap = new Dictionary<IStatusHandler, Status>();

        public bool IsDebuggerReady { get; private set; } = false;

        void Awake()
        {
            systemAsset = GetComponent<BehaviourRunner>().GetBehaviourSystemAsset();
            IsDebuggerReady = true;

            systemAsset.Graphs.ForEach(g =>
            {
                var dict = g.Graph.GetNodeNames();
                foreach (var node in g.Graph.NodeList)
                {
                    if (node is IStatusHandler handler)
                    {
                        handler.StatusChanged += (status) => DebugStatusChanged(dict.GetValueOrDefault(node) ?? "unnamed", status, g.Name);

                        handler.StatusChanged += (status) =>
                        {
                            if (status != Status.None) 
                                _lastFinishedStatusMap[handler] = status;
                        };
                    }
                }
            });
        }

        void DebugStatusChanged(string name, Status status, string graphName)
        {
            if (!debugStatusChanges) return;
            
            var colorTag = $"#{ColorUtility.ToHtmlStringRGB(status.ToColor())}";
            Debug.Log($"<color=cyan>[DEBUGGER]</color> - Node <color=cyan>{name}</color> changed to <b><color={colorTag}>{status}</color></b>\n" +
                $"Graph: {graphName} | Frame: {Time.frameCount}", this);
        }
    }
}

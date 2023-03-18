using BehaviourAPI.Core;
using BehaviourAPI.Unity.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    using UnityExtensions;

    [DefaultExecutionOrder(1000)]

    /// <summary>
    /// Component used for debug a behaviour system runner
    /// </summary>   
    public class BSRuntimeDebugger : MonoBehaviour, IBehaviourSystem
    {
        public SystemData Data { get; private set; }

        [SerializeField] bool debugStatusChanges;

        public bool IsDebuggerReady { get; private set; } = false;

        public Object ObjectReference => null;

        void Awake()
        {
            var runner = GetComponent<BehaviourRunner>();

            if (runner == null)
            {
                Debug.LogWarning("BSRuntimeDebugger need a BehaviourRunner component to work");
                return;
            }

            Data = runner.GetBehaviourSystemAsset();
            if (Data != null)
            {
                Data.graphs.ForEach(g =>
                {
                    var dict = g.graph.GetNodeNames();
                    foreach (var node in g.graph.NodeList)
                    {
                        if (node is IStatusHandler handler)
                        {
                            handler.StatusChanged += (status) => DebugStatusChanged(dict.GetValueOrDefault(node) ?? "unnamed", status, g.name);
                        }
                    }
                });
            }
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

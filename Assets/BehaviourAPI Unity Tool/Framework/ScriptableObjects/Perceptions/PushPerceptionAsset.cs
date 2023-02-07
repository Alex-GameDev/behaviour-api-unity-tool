using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    /// <summary>
    /// Stores a push perception as an unity object.
    /// </summary>
    public class PushPerceptionAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public string Name;

        [SerializeReference] public PushPerception pushPerception = new PushPerception();

        [HideInInspector][SerializeField] List<NodeAsset> targets = new List<NodeAsset>();       

        public List<NodeAsset> Targets
        {
            get => targets;
        }

        public static PushPerceptionAsset Create(string name)
        {
            var pushPerceptionAsset = CreateInstance<PushPerceptionAsset>();
            pushPerceptionAsset.Name = name;
            return pushPerceptionAsset;
        }

        public void OnAfterDeserialize()
        {
            targets.ForEach(t =>
            {
                if (t.Node is IPushActivable pushTarget)
                {
                    pushPerception.PushListeners.Add(pushTarget);
                }
                else
                    Debug.LogWarning("Deserialization error: node target is not an IPushActivable");

            });
        }

        public void OnBeforeSerialize()
        {
            return;
        }
    }
}

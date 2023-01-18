using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a push perception as an unity object.
    /// </summary>
    public class PushPerceptionAsset : ScriptableObject
    {
        public string Name;

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

        public PushPerception Build()
        {
            var pp = new PushPerception();
            targets.ForEach(t =>
            {
                if (t.Node is IPushActivable pushTarget) pp.PushListeners.Add(pushTarget);
            });
            return pp;
        }
    }
}

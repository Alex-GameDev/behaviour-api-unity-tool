using BehaviourAPI.Core.Perceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.Unity.Framework
{
    public class CompoundPerceptionAsset : PerceptionAsset
    {
        [HideInInspector] public List<PerceptionAsset> subperceptions = new List<PerceptionAsset>();

        public static CompoundPerceptionAsset CreateCompound(string name, Type type)
        {
            if (!type.IsSubclassOf(typeof(Perception))) return null;

            CompoundPerceptionAsset asset = CreateInstance<CompoundPerceptionAsset>();
            asset.Name = name;
            asset.perception = (Perception)Activator.CreateInstance(type);
            return asset;
        }

        public override void Build()
        {
            base.Build();
            if(perception is CompoundPerception compoundPerception)
                compoundPerception.Perceptions = subperceptions.Select(sp => sp.perception).ToList();
        }
    }
}
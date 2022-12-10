using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CompoundPerceptionAsset : PerceptionAsset
    {
        [SerializeField] List<PerceptionAsset> perceptions;

        public static CompoundPerceptionAsset Create(string name)
        {
            var compoundPerceptionAsset = CreateInstance<CompoundPerceptionAsset>();
            compoundPerceptionAsset.Name = name;
            return compoundPerceptionAsset;
        }
    }
}

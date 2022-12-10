using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public class CompoundPerceptionAsset : PerceptionAsset
    {
        [SerializeField] List<PerceptionAsset> perceptions;
    }
}

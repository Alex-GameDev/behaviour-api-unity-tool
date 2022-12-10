using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public class SubgraphActionAsset : ActionAsset
    {
        [SerializeField] GraphAsset subgraph;        
    }
}

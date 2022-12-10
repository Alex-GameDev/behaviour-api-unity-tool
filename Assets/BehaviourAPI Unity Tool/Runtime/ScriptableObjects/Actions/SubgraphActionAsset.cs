using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public class SubgraphActionAsset : ActionAsset
    {
        [SerializeField] GraphAsset subgraph;

        public static SubgraphActionAsset Create(string name)
        {
            var subgraphActionAsset = CreateInstance<SubgraphActionAsset>();
            subgraphActionAsset.Name = name;
            return subgraphActionAsset;
        }
    }
}

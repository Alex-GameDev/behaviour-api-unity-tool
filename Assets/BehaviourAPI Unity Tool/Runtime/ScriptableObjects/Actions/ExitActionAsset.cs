using BehaviourAPI.Core;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores a perception as an unity object.
    /// </summary>
    public abstract class ExitActionAsset : ActionAsset
    {
        [SerializeField] Status status;

        public static ExitActionAsset Create(string name)
        {
            var exitActionAsset = CreateInstance<ExitActionAsset>();
            exitActionAsset.Name = name;
            return exitActionAsset;
        }
    }
}

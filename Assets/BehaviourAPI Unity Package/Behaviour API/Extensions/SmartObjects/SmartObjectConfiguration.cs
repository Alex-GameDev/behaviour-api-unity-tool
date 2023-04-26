using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    [CreateAssetMenu(fileName = "New SmartObjectConfig", menuName = "BehaviourAPI/SmartObjects/Configuration")]
    public class SmartObjectConfiguration : ScriptableObject
    {
        [Header("Capabilities")]
        [SerializeField] CapabilityMap _capabilities;

        [Header("Tags")]
        [SerializeField] List<string> _tags;
    }
}

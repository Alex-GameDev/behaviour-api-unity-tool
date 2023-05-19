using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    [CreateAssetMenu(fileName = "New SmartObject Settings", menuName = "BehaviourAPI/Smart Objects/Smart Object Settings")]
    public class SmartObjectSettings : ScriptableObject
    {
        [Header("Capabilities")]
        [SerializeField] CapabilityMap _capabilities;

        [Header("Tags")]
        [SerializeField] List<string> _tags;

        public float GetCapability(string name)
        {
            return _capabilities.GetValueOrDefault(name);
        }

        public List<string> GetCapabilities()
        {
            return _capabilities.Keys.ToList();
        }
    }
}

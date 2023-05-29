using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    [CreateAssetMenu(fileName = "NewAgentSettings", menuName = "BehaviourAPI/Smart Objects/Agent Settings")]
    public class SmartAgentSettings : ScriptableObject
    {
        [Header("Needs")]
        [SerializeField] CapabilityMap _needMap;

        public Dictionary<string, float> GetCapabilityMap() => _needMap;
    }
}

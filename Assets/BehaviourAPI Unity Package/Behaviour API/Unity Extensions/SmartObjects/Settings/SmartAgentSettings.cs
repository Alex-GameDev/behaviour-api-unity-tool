using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    [CreateAssetMenu(fileName = "NewAgentSettings", menuName = "BehaviourAPI/Smart Objects/Agent Settings")]
    public class SmartAgentSettings : ScriptableObject
    {
        [Header("Needs")]
        [SerializeField] CapabilityMap _needMap;
    }
}

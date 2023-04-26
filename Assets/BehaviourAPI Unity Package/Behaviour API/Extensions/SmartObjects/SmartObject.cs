using BehaviourAPI.SmartObjects;
using BehaviourAPI.UnityExtensions;
using UnityEngine;

namespace BehaviourAPI.Unity.SmartObjects
{
    public abstract class SmartObject : MonoBehaviour, ISmartObject
    {
        [SerializeField] SmartObjectConfiguration _config;

        public float GetCapability(string name)
        {
            throw new System.NotImplementedException();
        }

        public void RequestInteraction()
        {
            throw new System.NotImplementedException();
        }

        public bool ValidateAgent()
        {
            throw new System.NotImplementedException();
        }
    }
}

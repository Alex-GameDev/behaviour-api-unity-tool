using BehaviourAPI.UnityToolkit.SmartObjects;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    [DefaultExecutionOrder(-1)]
    public class SmartObjectManager : MonoBehaviour
    {
        public static SmartObjectManager Instance { get; private set; }

        public List<SmartObject> RegisteredObjects { get; private set; } = new List<SmartObject>();


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }


        public void RegisterSmartObject(SmartObject smartObject)
        {
            RegisteredObjects.Add(smartObject);
        }

        public void UnregisterSmartObject(SmartObject smartObject)
        {
            RegisteredObjects.Remove(smartObject);
        }
    }
}

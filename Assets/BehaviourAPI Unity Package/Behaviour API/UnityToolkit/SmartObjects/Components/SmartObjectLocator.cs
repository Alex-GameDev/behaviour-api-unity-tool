using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit.SmartObjects
{
    public class SmartObjectLocator : MonoBehaviour
    {
        public bool autoCheck = true;
        public float intervalTime = 1f;
        public float maxDistance = 10f;

        List<SmartObject> _availableSmartObjects;

        float _timer;

        private void Start()
        {
            _availableSmartObjects = new List<SmartObject>();
        }

        private void Update()
        {
            if (autoCheck) return;

            // Actualizar el temporizador
            _timer += Time.deltaTime;

            // Verificar si ha pasado el tiempo suficiente para realizar una comprobación
            if (_timer >= intervalTime)
            {
                LocateObjects();                
            }
        }

        public void LocateObjects()
        {
            _availableSmartObjects.Clear();

            SmartObject[] allSmartObjects = FindObjectsOfType<SmartObject>();

            foreach (SmartObject obj in allSmartObjects)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance <= maxDistance)
                {
                    _availableSmartObjects.Add(obj);
                }
            }
            _timer = 0; 
        }

        public List<SmartObject> GetSmartObjects() => _availableSmartObjects;
    }
}

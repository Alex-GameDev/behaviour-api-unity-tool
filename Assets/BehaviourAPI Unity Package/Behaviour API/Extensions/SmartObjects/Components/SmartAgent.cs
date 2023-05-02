using BehaviourAPI.Core;
using BehaviourAPI.SmartObjects;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityExtensions
{
    public class SmartAgent : MonoBehaviour, ISmartAgent
    {
        [SerializeField] SmartAgentSettings _settings;

        Dictionary<string, float> m_Needs;

        private void Awake()
        {
            m_Needs = new Dictionary<string, float>();
        }

        public float GetNeed(string name)
        {
            return m_Needs.GetValueOrDefault(name);
        }

        public void CoverNeed(string name, float value)
        {
            if (m_Needs.ContainsKey(name))
            {
                m_Needs[name] += value;
                OnNeedCovered(name, value);
            }
        }

        protected virtual void OnNeedCovered(string name, float value)
        {
        }

        public Status Move(Transform target)
        {
            transform.position = target.position;
            transform.rotation = target.rotation;
            return Status.Success;
        }


    }
}

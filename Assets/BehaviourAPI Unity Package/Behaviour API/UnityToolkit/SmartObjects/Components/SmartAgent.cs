using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    using SmartObjects;

    /// <summary>
    /// Unity component that implements the ISmartAgent interface.
    /// </summary>
    public class SmartAgent : MonoBehaviour, ISmartAgent
    {
        [SerializeField] SmartAgentSettings _settings;
        public BehaviourRunner Runner { get; private set; }

        /// <summary>
        /// Component used for the agent movement.
        /// </summary>
        public IMovementComponent Movement { get; private set; }

        /// <summary>
        /// Component used for the agent to talk.
        /// </summary>
        public ITalkComponent Talk { get; set; }

        /// <summary>
        /// Component used fot the agent 
        /// </summary>

        Dictionary<string, float> m_Needs;

        private void Awake()
        {
            m_Needs = _settings.GetCapabilityMap();
            Runner = GetComponent<BehaviourRunner>();
            Movement = GetComponent<IMovementComponent>();
            Talk = GetComponent<ITalkComponent>();
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
            }
        }
    }
}

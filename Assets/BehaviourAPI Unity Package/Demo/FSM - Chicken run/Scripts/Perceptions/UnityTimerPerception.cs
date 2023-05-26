using UnityEngine;

namespace BehaviourAPI.UnityToolkit
{
    /// <summary>
    /// Perception triggered when a totalTime passes, asuming its executing on Update
    /// </summary>
    public class UnityTimePerception : UnityPerception
    {
        public float TotalTime;

        float _currentTime;

        public UnityTimePerception()
        {
        }

        public UnityTimePerception(float totalTime)
        {
            TotalTime = totalTime;
        }

        public override void Initialize()
        {
            _currentTime = 0f;
        }

        public override void Reset()
        {
            _currentTime = 0f;
        }

        public override bool Check()
        {
            _currentTime += Time.deltaTime;
            return _currentTime >= TotalTime;
        }

        public override string DisplayInfo => "$TotalTime second(s) passes";
    }
}
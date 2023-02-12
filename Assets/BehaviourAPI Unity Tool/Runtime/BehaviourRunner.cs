using UnityEngine;
using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class BehaviourRunner : MonoBehaviour
    {     
        public abstract BehaviourSystemAsset GetBehaviourSystemAsset();

        private void Awake() => OnAwake();

        private void Start() => OnStart();

        private void Update() => OnUpdate();

        protected abstract void OnAwake();

        protected abstract void OnStart();

        protected abstract void OnUpdate();
    }
}

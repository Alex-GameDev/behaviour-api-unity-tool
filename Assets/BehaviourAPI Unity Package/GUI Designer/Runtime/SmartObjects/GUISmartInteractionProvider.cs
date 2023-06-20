using UnityEngine;

namespace BehaviourAPI.UnityToolkit.GUIDesigner.Runtime
{
    using Core.Actions;
    using Framework;

    public class GUISmartInteractionProvider : DataSmartInteractionProvider, IBehaviourSystem
    {
        [SerializeField] SystemData data;
        public SystemData Data => data;
        public Object ObjectReference => this;

        private BehaviourSystem _bSystem;

        private void Awake()
        {
            _bSystem = BehaviourSystem.CreateSystem(data);
        }

        protected override SystemData GetSystemdata()
        {
            return _bSystem.GetBehaviourSystemData();
        }
    }
}

namespace BehaviourAPI.Unity.Runtime
{
    using Framework;
    using UnityEngine;

    /// <summary>
    /// Subclass of  <see cref="BehaviourRunner"/> that executes a reusable <see cref="BehaviourSystem"/> 
    /// </summary>
    public abstract class AssetBehaviourRunner : DataBehaviourRunner, IBehaviourSystem
    {
        public BehaviourSystem System;
        SystemData _runtimeSystem = null;

        public SystemData Data => _runtimeSystem;

        public Object ObjectReference => this;

        /// <summary>
        /// Returns the system asset data to generate a runtime copy
        /// </summary>
        protected sealed override SystemData GetEditedSystemData()
        {
            _runtimeSystem = System.GetBehaviourSystemData();
            return _runtimeSystem;
        }
    }
}

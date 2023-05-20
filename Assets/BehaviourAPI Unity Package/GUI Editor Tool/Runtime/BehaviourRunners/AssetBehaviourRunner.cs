namespace BehaviourAPI.Unity.Runtime
{
    using Framework;
    using UnityEngine;

    /// <summary>
    /// Subclass of  <see cref="BehaviourRunner"/> that executes a reusable <see cref="BehaviourSystem"/> 
    /// </summary>
    public abstract class AssetBehaviourRunner : DataBehaviourRunner
    {
        public BehaviourSystem System;
        SystemData _runtimeSystem;

        /// <summary>
        /// Returns the system asset data to generate a runtime copy
        /// </summary>
        protected sealed override SystemData GetEditorSystemData()
        {
            string json = JsonUtility.ToJson(System);
            BehaviourSystem copy = ScriptableObject.CreateInstance<BehaviourSystem>();
            JsonUtility.FromJsonOverwrite(json, copy);
            _runtimeSystem = copy.Data;
            return _runtimeSystem;
        }

        public sealed override SystemData GetBehaviourSystemAsset() => _runtimeSystem;
    }
}

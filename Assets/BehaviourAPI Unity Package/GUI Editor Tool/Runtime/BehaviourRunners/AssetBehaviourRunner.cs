using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Subclass of  <see cref="BehaviourRunner"/> that executes a reusable <see cref="BehaviourSystem"/> 
    /// </summary>
    public abstract class AssetBehaviourRunner : DataBehaviourRunner
    {      
        public BehaviourSystem System;

        /// <summary>
        /// Returns the system asset data to generate a runtime copy
        /// </summary>
        protected override SystemData GetEditorSystemData() => System.Data;
    }
}

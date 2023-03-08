using BehaviourAPI.Unity.Framework;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Subclass of  <see cref="BehaviourRunner"/> that executes a reusable <see cref="BehaviourSystemAsset"/> 
    /// </summary>
    public abstract class AssetBehaviourRunner : DataBehaviourRunner
    {      
        public BehaviourSystemAsset System;

        /// <summary>
        /// Returns the system asset to generate a runtime copy
        /// </summary>
        protected override BehaviourSystemAsset GetEditorSystem() => System;
    }
}

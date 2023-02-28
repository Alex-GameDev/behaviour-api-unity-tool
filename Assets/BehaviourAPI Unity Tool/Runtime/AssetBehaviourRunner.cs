using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    public abstract class AssetBehaviourRunner : DataBehaviourRunner
    {      
        public BehaviourSystemAsset System;

        /// <summary>
        /// Returns the system asset to generate a runtime copy
        /// </summary>
        protected override BehaviourSystemAsset GetEditorSystem() => System;
    }
}

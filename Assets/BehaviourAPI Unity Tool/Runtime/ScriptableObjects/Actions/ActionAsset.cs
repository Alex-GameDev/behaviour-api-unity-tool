using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
    /// <summary>
    /// Stores an action as an unity object
    /// </summary>
    public abstract class ActionAsset : ScriptableObject
    {
        public static ActionAsset Create(Type type)
        {
            if (type.IsSubclassOf(typeof(ActionAsset))) return null;

            var actionAsset = (ActionAsset) CreateInstance(type);
            return actionAsset;
        }
    }
}

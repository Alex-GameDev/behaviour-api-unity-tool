using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAPI.Unity.Runtime
{
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

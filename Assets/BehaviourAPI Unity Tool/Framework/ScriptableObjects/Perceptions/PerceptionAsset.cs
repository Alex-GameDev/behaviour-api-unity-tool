using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionAsset : ScriptableObject
{
    [SerializeReference] public Perception perception;
    public string Name;

    public static PerceptionAsset Create(string name, Type type)
    {
        if (!type.IsSubclassOf(typeof(Perception))) return null;

        PerceptionAsset asset;
        if (type.IsAssignableFrom(typeof(CompoundPerception)))
        {
            asset = CreateInstance<CompoundPerceptionAsset>();
        }
        else if (type.IsAssignableFrom(typeof(ExecutionStatusPerception)))
        {
            asset = CreateInstance<StatusPerceptionAsset>();
        }
        else
            asset = CreateInstance<PerceptionAsset>();          

        asset.Name = name;
        asset.perception = (Perception) Activator.CreateInstance(type);
        return asset;
    }
}

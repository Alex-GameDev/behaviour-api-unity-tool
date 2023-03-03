using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Framework;
using BehaviourAPI.Unity.Framework.Adaptations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionAsset : ScriptableObject
{
    public string Name;

    [SerializeReference] public Perception perception;

    public static PerceptionAsset Create(string name, Type type)
    {
        if (!type.IsSubclassOf(typeof(Perception))) return null;

        PerceptionAsset asset;
        if (type.IsSubclassOf(typeof(CompoundPerception)))
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

    public virtual void Build()
    {
        return;
    }
}

using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Perception triggered when a time passes, asuming its executing on Update
/// </summary>
public class UnityTimePerception : BehaviourAPI.Core.Perceptions.Perception
{
    public float TotalTime;

    float _currentTime;

    public UnityTimePerception(float time)
    {
        TotalTime = time;
    }

    public override void Initialize()
    {
        _currentTime = 0f;
    }

    public override void Reset()
    {
        _currentTime = 0f;
    }

    public override bool Check()
    {
        _currentTime += Time.deltaTime;
        return _currentTime >= TotalTime;
    }
}

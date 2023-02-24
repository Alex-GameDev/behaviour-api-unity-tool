using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CarFSMAssetRunner : AssetBehaviourRunner, ICar
{
    Rigidbody _rb;

    protected override void OnAwake()
    {
        _rb = GetComponent<Rigidbody>();

        base.OnAwake();
        IRadar radar = GameObject.FindGameObjectWithTag("Radar").GetComponent<IRadar>();

        FindPerception<ExecutionStatusPerception>("radar is broken").StatusHandler = radar.GetBrokenState();
        FindPerception<ExecutionStatusPerception>("radar is fixed").StatusHandler = radar.GetWorkingState();  
    }

    public float GetSpeed() => _rb.velocity.magnitude;
}

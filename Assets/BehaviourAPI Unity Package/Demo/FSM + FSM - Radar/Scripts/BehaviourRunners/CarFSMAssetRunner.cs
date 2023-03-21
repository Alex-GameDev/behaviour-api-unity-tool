using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.StateMachines;
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

        var mainGraph = FindGraph("main");
        var speedUp = mainGraph.FindNode<StateTransition>("speed up");
        var speedDown = mainGraph.FindNode<StateTransition>("speed down");

        speedUp.Perception = new ExecutionStatusPerception(radar.GetBrokenState(), StatusFlags.Running);
        speedDown.Perception = new ExecutionStatusPerception(radar.GetWorkingState(), StatusFlags.Running);
    }

    public float GetSpeed() => _rb.velocity.magnitude;
}

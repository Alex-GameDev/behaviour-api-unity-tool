
using BehaviourAPI.Core;
using BehaviourAPI.Core.Actions;
using BehaviourAPI.Core.Perceptions;
using BehaviourAPI.Unity.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarFSMRunner : CodeBehaviourRunner
{
    Rigidbody _rb;
    RadarFSMRunner _radar;

    float _speed;
    protected override void OnAwake()
    {
        _rb = GetComponent<Rigidbody>();
        _radar = FindObjectOfType<RadarFSMRunner>();
        _speed = Random.Range(15f, 30f);
        base.OnAwake();
    }

    protected override BehaviourGraph CreateGraph()
    {
        var carFSM = new BehaviourAPI.StateMachines.FSM();

        // Esta percepción se activará cuando el estado del radar sea "broken"
        var radarIsBroken = new ExecutionStatusPerception(_radar.GetBrokenState());

        // Esta percepción se activará cuando el estado del radar sea "working"
        var radarIsWorking = new ExecutionStatusPerception(_radar.GetWorkingState());

        var lowSpeedState = carFSM.CreateState("low speed", new FunctionalAction(() => _rb.velocity = transform.forward * _speed));
        var highSpeedState = carFSM.CreateState("high speed", new FunctionalAction(() => _rb.velocity = transform.forward * (_speed + 10f)));

        // La FSM cambia de un estado a otro con el paso del tiempo:
        carFSM.CreateTransition("speed up", lowSpeedState, highSpeedState, radarIsBroken);
        carFSM.CreateTransition("slow down", highSpeedState, lowSpeedState, radarIsWorking);

        RegisterGraph(carFSM);
        return carFSM;
    }

    public float GetSpeed() => _rb.velocity.magnitude;
}

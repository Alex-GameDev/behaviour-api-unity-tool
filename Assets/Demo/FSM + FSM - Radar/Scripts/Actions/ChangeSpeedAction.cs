using BehaviourAPI.Core;
using BehaviourAPI.Unity.Runtime;
using BehaviourAPI.Unity.Runtime.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionGroup("DEMO - Radar")]
public class ChangeSpeedAction : UnityAction
{
    public float baseSpeed;
    public float minAddedSpeed;
    public float maxAddedSpeed;

    public ChangeSpeedAction(float baseSpeed, float minRandomAddedSpeed, float maxRandomAddedSpeed)
    {
        this.baseSpeed = baseSpeed;
        this.minAddedSpeed = minRandomAddedSpeed;
        this.maxAddedSpeed = maxRandomAddedSpeed;
    }

    public ChangeSpeedAction()
    {
    }

    public override void Start()
    {
        var s = baseSpeed + Random.Range(minAddedSpeed, maxAddedSpeed) + baseSpeed;
        context.Rigidbody.velocity = context.Transform.forward * s;
    }

    public override void Stop()
    {
    }

    public override Status Update()
    {
        return Status.Success;
    }
}

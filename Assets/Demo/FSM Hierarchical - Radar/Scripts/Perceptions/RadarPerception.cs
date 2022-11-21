using System;
using System.Collections;
using System.Collections.Generic;
using BehaviourAPI.Core;
using BehaviourAPI.Core.Perceptions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// Perception triggered when radar detects a car and the speedCheckFunction returns true
/// </summary>
public class RadarPerception : BehaviourAPI.Core.Perceptions.Perception
{
    public Vector3 PointToLook;
    public Transform Origin;
    public Func<float, bool> SpeedCheckFunction;

    public Text Marker;
    public RadarPerception(Vector3 pointToLook, Transform origin, Func<float, bool> speedCheckFunction, Text marker)
    {
        PointToLook = pointToLook;
        Origin = origin;
        SpeedCheckFunction = speedCheckFunction;
        Marker = marker;
    }

    public override bool Check()
    {
        Ray ray = new Ray(Origin.position, -Origin.TransformPoint(PointToLook));

        if (Physics.Raycast(ray, out RaycastHit hit, 50) && hit.collider.tag == "Car")
        {
            var carSpeed = hit.collider.gameObject.GetComponent<CarFSMRunner>().GetSpeed();

            bool trigger = SpeedCheckFunction?.Invoke(carSpeed) ?? false;
            if (trigger)
            {
                Marker.text = $"{Mathf.RoundToInt(carSpeed) + 100}";
            }
            return trigger;

        }
        return false;
    }
}
